using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace HotelBilling.Tests
{
    [TestFixture]
    public class HotelBillingTests
    {
        private HotelValidation validation;
        private HotelBillingService billing;

        [SetUp]
        public void Setup()
        {
            Dictionary<string, decimal> amenities =new Dictionary<string, decimal>
                {
                    { "WIFI", 0 },
                    { "SPA", 1500 },
                    { "GYM", 500 },
                    { "POOL", 800 }
                };

            validation = new HotelValidation(amenities);
            string json =
                """
                {
                  "amenities": [
                    { "code": "WIFI", "price": 0 },
                    { "code": "SPA", "price": 1500 },
                    { "code": "GYM", "price": 500 },
                    { "code": "POOL", "price": 800 }
                  ]
                }
                """;
            File.WriteAllText("testAmenities.json", json);

            billing =new HotelBillingService("testAmenities.json");
        }

        [TearDown]
        public void Cleanup()
        {
            if (File.Exists("testAmenities.json"))
                File.Delete("testAmenities.json");

            if (File.Exists("testRooms.csv"))
                File.Delete("testRooms.csv");
        }

        [Test]
        public void StandardRoomBilling()
        {
            Reservation reservation =new Reservation("R1", "Standard", 2, "");
            decimal result =validation.CalculateTotal(reservation);
            Assert.That(result, Is.EqualTo(4000));
        }

        [Test]
        public void DeluxeRoomWithMultipleAmenities()
        {
            Reservation reservation = new Reservation("R1", "Deluxe", 3, "WIFI;SPA");
            decimal result =validation.CalculateTotal(reservation);
            Assert.That(result, Is.EqualTo(12000));
        }

        [Test]
        public void ZeroNightsThrowsException()
        {
            Reservation reservation = new Reservation("R1", "Suite", 0, "");
            Assert.Throws<InvalidNightsException>(() => validation.CalculateTotal(reservation));
        }

        [Test]
        public void NegativeNightsThrowsException()
        {
            Reservation reservation =new Reservation("R1", "Suite", -2, "");

            Assert.Throws<InvalidNightsException>( () => validation.CalculateTotal(reservation));
        }

        [Test]
        public void UnknownAmenityThrowsException()
        {
            Reservation reservation =new Reservation("R1", "Deluxe", 2, "ABC");
            Assert.Throws<UnknownAmenityException>(() => validation.CalculateTotal(reservation));
        }

        [Test]
        public void UnknownRoomTypeThrowsException()
        {
            Reservation reservation =new Reservation("R1", "Premium", 2, "");

            Assert.Throws<UnknownRoomTypeException>(() => validation.CalculateTotal(reservation));
        }

        [Test]
        public void DuplicateReservationIsSkipped()
        {
            string csv =
                "ReservationId,RoomType,Nights,AmenityCodes\n" +
                "R1,Standard,2,\n" +
                "R1,Deluxe,3,\n";

            File.WriteAllText("testRooms.csv", csv);
            List<Reservation> result =billing.ReadReservations("testRooms.csv");
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void CorrectTotalCalculation()
        {
            Reservation reservation = new Reservation("R1", "Suite", 5, "SPA;GYM");
            decimal result = validation.CalculateTotal(reservation);
            Assert.That(result, Is.EqualTo(27000));
        }

        [Test]
        public void BinaryEncodeDecodeRoundTrip()
        {
            Reservation reservation =new Reservation("R1", "Deluxe", 3, "WIFI;SPA");

            reservation.Total =validation.CalculateTotal(reservation);

            List<Reservation> records = new List<Reservation> { reservation };

            using MemoryStream memory =billing.StageRecords(records);
            List<Reservation> result = billing.VerifyRecords(memory);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo("R1"));
            Assert.That(result[0].RoomType, Is.EqualTo("Deluxe"));
            Assert.That(result[0].Nights, Is.EqualTo(3));
            Assert.That(result[0].Total, Is.EqualTo(12000));
        }

        [Test]
        public void EmptyCsvReturnsNoRecords()
        {
            string csv ="ReservationId,RoomType,Nights,AmenityCodes\n";
            File.WriteAllText("testRooms.csv", csv);

            List<Reservation> result = billing.ReadReservations("testRooms.csv");
            Assert.That(result.Count, Is.EqualTo(0));
        }
    }
}