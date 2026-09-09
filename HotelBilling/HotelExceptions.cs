using System;
using System.Collections.Generic;
using System.Text;

namespace HotelBilling
{
   
        public class HotelException : Exception
        {
            public HotelException(string message) : base(message) { }
        }
        public class InvalidNightsException : HotelException
        {
            public InvalidNightsException() : base("Nights must be greater than 0.") { }
        }
        public class UnknownAmenityException : HotelException
        {
            public UnknownAmenityException(string code) : base("Unknown amenity: " + code) { }
        }
        public class UnknownRoomTypeException : HotelException
        {
            public UnknownRoomTypeException(string type): base("Unknown room type: " + type) { }
        }
        public class DuplicateReservationException : HotelException
        {
            public DuplicateReservationException(string id): base("Duplicate reservation: " + id) { }
        }
    
}
