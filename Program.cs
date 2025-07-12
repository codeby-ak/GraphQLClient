using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HttpClientExample
{
    class Program
    {
        static async Task Main(string[] args)
        {


            //Console.WriteLine("Enter Sales Order ID:");
            //string input = Console.ReadLine();

            //if (int.TryParse(input, out orderid)) // assign command-line value to myValue
            //{
            //    Console.WriteLine("Fetching sales order details....");
            //}
            //else
            //{
            //    Console.WriteLine("Invalid integer value.");
            //}
            //int orderid; //43659
            //43659
            //43660
            //43661
            //43662
            //43663
            //43664
            //43665

            while (true)
            {
                Console.WriteLine("Enter Sales Order ID:");
                string orderid = Console.ReadLine();

                if (string.Equals(orderid, "exit", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Exiting the program.");
                    break;
                }

                if (int.TryParse(orderid, out int number))
                {
                    await GetOrder(number);
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a valid integer.");
                }
            }

        }

        private static async Task GetOrder(int orderid)
        {
            using (var client = new HttpClient())
            {
                var query = new
                {
                    query = @"query Sales($id: Int!) {
                            salesOrderById(id: $id) {
                                orderQty
                                productId
                                rowguid
                                salesOrderDetailId
                                salesOrderId
                                specialOfferId
                                unitPrice
                                unitPriceDiscount
                                product {
                                    name
                                    listPrice   
                                }
                            }
                        }",
                    variables = new { id = orderid }
                };

                var json = System.Text.Json.JsonSerializer.Serialize(query);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("http://localhost:5190/graphql/", content);
                var result = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var de_result = JsonSerializer.Deserialize<GraphQLResponse>(result, options);

                Console.WriteLine("Sales Order Details\n--------------------");

                foreach (var item in de_result.Data.ProductWithSalesOrder)
                {
                    Console.WriteLine($"SalesOrderDetailId: {item.SalesOrderDetailId}");
                    Console.WriteLine($"Product ID         : {item.ProductId}");
                    Console.WriteLine($"Product Name       : {item.Product.Name}");
                    Console.WriteLine($"List Price         : {item.Product.listPrice}");
                    Console.WriteLine($"Quantity           : {item.OrderQty}");
                    Console.WriteLine($"Unit Price         : {item.UnitPrice:C}");
                    Console.WriteLine($"Discount           : {item.UnitPriceDiscount:P}");
                    Console.WriteLine($"Row GUID           : {item.Rowguid}");
                    Console.WriteLine(new string('-', 60));
                }

                //Console.WriteLine(result);
                Console.ReadLine();
            }
        }
    }

    // Classes matching the JSON structure
    public class GraphQLResponse
    {
        public GraphQLData Data { get; set; }
    }

    public class GraphQLData
    {
        [JsonPropertyName("salesOrderById")]
        public List<ProductSalesOrder> ProductWithSalesOrder { get; set; }
    }

    public class ProductSalesOrder
    {
        public int SalesOrderDetailId { get; set; }
        public int SalesOrderId { get; set; }
        public int ProductId { get; set; }
        public int SpecialOfferId { get; set; }
        public int OrderQty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal UnitPriceDiscount { get; set; }
        public string Rowguid { get; set; }
        public Product Product { get; set; }
    }

    public class Product
    {
        public string Name { get; set; }
        public decimal listPrice { get; set; }
    }
}