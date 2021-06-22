namespace biz.serena.Models.PushNotification
{
    public class PushNotificationRequest
    {
        public string to { get; set; }
        public string priority { get; set; }
        public PNotification notification { get; set; }
        public DataNotification data { get; set; }

        public class PNotification
        {
            public string title { get; set; }
            public string text { get; set; }            
            public string click_action { get; set; }
            public string sound { get; set; }

        }

        public class DataNotification
        {
            public long id_notificacion { get; set; }
            public string link { get; set; }
            public int conversationId { get; set; }
            public int action { get; set; }
            public string click_action { get; set; }
            public string title { get; set; }
            public string text { get; set; }
            public string sound { get; set; }
        }

        public PushNotificationRequest()
        {
            notification = new PNotification();
            data = new DataNotification();
        }
    }
}