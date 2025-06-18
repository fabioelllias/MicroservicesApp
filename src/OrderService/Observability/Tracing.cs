using System.Diagnostics;

namespace OrderService.Observability
{
    public static class Tracing
    {
        public static readonly ActivitySource Source = new("OrderService");
    }
}
