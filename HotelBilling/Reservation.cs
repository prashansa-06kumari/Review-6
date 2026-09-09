using System;
using System.Collections.Generic;
using System.Text;

namespace HotelBilling
{
    public class Reservation
    {
        public string Id { get; set; }
        public string RoomType { get; set; }
        public int Nights { get; set; }
        public string AmenityCodes { get; set; }
        public decimal Total { get; set; }
        public Reservation(string id, string roomType, int nights, string amenityCodes)
        {
            Id=id;
            RoomType=roomType;
            Nights=nights;
            AmenityCodes=amenityCodes;
        }

    }
}
