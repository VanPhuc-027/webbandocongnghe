namespace _2280613193_webdocongnghe.Helpers
{ 
        public static class OrderStatusFlow
        {
            public static readonly Dictionary<string, List<string>> ValidTransitions = new()
        {
            { "Chờ xác nhận", new List<string> { "Đã xác nhận" } },
            { "Đã xác nhận", new List<string> { "Đang giao" } },
            { "Đang giao", new List<string> { "Đã giao" } },
            { "Đã giao", new List<string>() },
            { "Đã hủy", new List<string>() },
            { "Trả lại hàng", new List<string>() }
        };
        }
}
