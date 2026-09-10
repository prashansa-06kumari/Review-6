using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HotelBilling
{
    public class HotelBillingService
    {
        private HotelValidation validation;
        public HotelBillingService(string amenityFile)
        {
            Dictionary<string, decimal> amenities = new Dictionary<string, decimal>();

            using FileStream fs =new FileStream(amenityFile, FileMode.Open);
            using StreamReader reader = new StreamReader(fs);
            string json = reader.ReadToEnd();
            using JsonDocument document= JsonDocument.Parse(json);
            foreach (JsonElement item in document.RootElement.GetProperty("amenities").EnumerateArray())
            {
                string code = item.GetProperty("code").GetString();
                decimal price =item.GetProperty("price").GetDecimal();
                amenities[code] = price;
            }
            validation = new HotelValidation(amenities);
        }
        public List<Reservation> ReadReservations(string csvFile)
        {
            List<Reservation> records = new List<Reservation>();
            HashSet<string> ids = new HashSet<string>();
            using FileStream fs =new FileStream(csvFile, FileMode.Open);
            using StreamReader reader =new StreamReader(fs);
            reader.ReadLine();
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                try
                {
                    string[] data = line.Split(',');

                    if (data.Length != 4)
                    {
                        Console.WriteLine("Malformed record skipped.");
                        continue;
                    }
                    string id = data[0].Trim();
                    string roomType = data[1].Trim();
                    if (string.IsNullOrWhiteSpace(id))
                    {
                        Console.WriteLine("Invalid reservation ID.");
                        continue;
                    }
                    if (!ids.Add(id))
                        throw new DuplicateReservationException(id);

                    if (!int.TryParse(data[2].Trim(), out int nights))
                        throw new InvalidNightsException();
                    Reservation reservation = new Reservation(id, roomType, nights, data[3].Trim());
                    reservation.Total = validation.CalculateTotal(reservation);
                    records.Add(reservation);

                }
                catch (HotelException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return records;

        }
        public MemoryStream StageRecords(List<Reservation> records)
        {
            MemoryStream memory =new MemoryStream();

            using BinaryWriter writer=new BinaryWriter( memory,Encoding.UTF8,true);

            foreach (Reservation reservation in records)
            {
                writer.Write(reservation.Id);
                writer.Write(reservation.RoomType);
                writer.Write(reservation.Nights);
                writer.Write(reservation.AmenityCodes);
                writer.Write(reservation.Total);
            }
            memory.Position = 0;
            return memory;
        }

        public List<Reservation> VerifyRecords(
            MemoryStream memory)
        {
            List<Reservation> verified =new List<Reservation>();
            using BinaryReader reader =new BinaryReader( memory,Encoding.UTF8,true);
            while (memory.Position < memory.Length)
            {
                string id = reader.ReadString();
                string roomType = reader.ReadString();
                int nights=reader.ReadInt32();
                string amenities =reader.ReadString();
                decimal storedTotal=reader.ReadDecimal();

                Reservation reservation =new Reservation(id,roomType,nights,amenities);
                decimal calculatedTotal =validation.CalculateTotal(reservation);
                if (calculatedTotal==storedTotal)
                {
                    reservation.Total=storedTotal;
                    verified.Add(reservation);
                }
            }
            return verified;
        }
        public void WriteBillingFile(
            List<Reservation> records, string file)
        {
            using FileStream fs =new FileStream(file, FileMode.Create);
            JsonSerializer.Serialize(fs,records,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });
        }
        public void WriteRevenueReport(
            List<Reservation> records,string file)
        {
            decimal revenue = 0;
            foreach (Reservation reservation in records)
            {
                revenue += reservation.Total;
            }
            using FileStream fs =new FileStream(file, FileMode.Create);
            using BufferedStream bufferedStream = new BufferedStream(fs);
            using StreamWriter writer =new StreamWriter(bufferedStream);
            writer.WriteLine("Daily Revenue Report");
            writer.WriteLine("Total Revenue: " + revenue);
        }

    }
}
