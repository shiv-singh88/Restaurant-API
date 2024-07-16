using Amazon.DynamoDBv2.DataModel;

namespace RestaurantApiHandler
{
    public class Restaurant
    {
        [DynamoDBProperty("id")]
        public int Id { get; set; }

        [DynamoDBProperty("name")]
        public string Name { get; set; }

        [DynamoDBProperty("address")]
        public string Address { get; set; }

        [DynamoDBProperty("description")]
        public string Description { get; set; }

        [DynamoDBProperty("hours")]
        public string Hours { get; set; }

        [DynamoDBProperty("averagerating")]
        public double AverageRating { get; set; }

    }
}
