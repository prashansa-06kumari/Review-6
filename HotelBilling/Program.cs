using System;
using System.Collections.Generic;
using System.Text;

using HotelBilling;


namespace HotelBilling
{
    public class Program
    {
        public static void Main()
        {
            HotelBillingService billing =new HotelBillingService("amenities.json");
            List<Reservation> records =billing.ReadReservations("rooms.csv");
            using MemoryStream staged =billing.StageRecords(records);
            List<Reservation> verified = billing.VerifyRecords(staged);

            billing.WriteBillingFile(verified,"billed_reservations.json");

            billing.WriteRevenueReport(verified,"daily_revenue.txt");

            Console.WriteLine();
            Console.WriteLine("Billing completed.");
            Console.WriteLine("Verified records: " + verified.Count);
        }
    }
}