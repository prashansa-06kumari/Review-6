using System;
using System.Collections.Generic;
using System.Text;
using HotelBilling;

namespace HotelBilling
{
    public class HotelValidation
    {
        private Dictionary<string, decimal> roomRates = new Dictionary<string, decimal>
           {
                { "Standard", 2000 },
                { "Deluxe", 3500 },
                { "Suite", 5000 }
           };
        private Dictionary<string, decimal> amenityPrices = new Dictionary<string, decimal>();
        public HotelValidation(Dictionary<string, decimal> amenities)
        {
            amenityPrices = amenities;
        }

        public decimal CalculateTotal(Reservation reservation)
        {
            if (reservation.Nights <= 0)throw new InvalidNightsException();
            if (!roomRates.ContainsKey(reservation.RoomType))throw new UnknownRoomTypeException(reservation.RoomType);
            decimal total =roomRates[reservation.RoomType]*reservation.Nights;
            if (!string.IsNullOrWhiteSpace(reservation.AmenityCodes))
            {
                string[] codes=reservation.AmenityCodes.Split(';');
                foreach (string code in codes)
                {
                    string amenity = code.Trim();
                    if (!amenityPrices.ContainsKey(amenity))
                    {
                        throw new UnknownAmenityException(amenity);
                    }
                    total += amenityPrices[amenity];
                }
            }
            return total;
            
        }

    }
}
