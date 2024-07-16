using Amazon.Lambda.Core;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.DynamoDBv2.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace RestaurantApiHandler;

public class Function
{
    private readonly AmazonDynamoDBClient _dynamoDbClient;
    public Function()
    {
        _dynamoDbClient = new AmazonDynamoDBClient();
    }
    public async Task<APIGatewayProxyResponse> FunctionHandlerAsync(APIGatewayProxyRequest request, ILambdaContext context)
    {
        // Handle API requests here
        return request.HttpMethod switch
        {
            "GET" => await GetRestaurants(request, context),
            "POST" => await CreateRestaurant(request, context),
            "PUT" => await UpdateRestaurant(request, context),
            "DELETE" => await DeleteRestaurant(request, context),
            _ => new APIGatewayProxyResponse
            {
                StatusCode = 405,
                Body = "Method not allowed"
            },
        };
    }
    private async Task<APIGatewayProxyResponse> GetRestaurants(APIGatewayProxyRequest request, ILambdaContext context)
    {
        // Get restaurants from DynamoDB
        var requestParams = new ScanRequest
        {
            TableName = "restaurants"
        };

        try
        {
            var response = await _dynamoDbClient.ScanAsync(requestParams);
            var restaurants = response.Items.Select(item => new Restaurant
            {
                Id = Convert.ToInt32(item["id"].S),
                Name = item["name"].S,
                Address = item["address"].S,
                Description = item["description"].S,
                Hours = item["hours"].S,
                AverageRating = Convert.ToDouble(item["averagerating"].S)
            });

            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = JsonConvert.SerializeObject(restaurants)
            };
        }
        catch (Exception ex)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = $"Error fetching restaurants: {ex.Message}"
            };
        }
    }

    private async Task<APIGatewayProxyResponse> CreateRestaurant(APIGatewayProxyRequest request, ILambdaContext context)
    {
        // Create a new restaurant in DynamoDB
        var restaurant = JsonConvert.DeserializeObject<Restaurant>(request.Body);

        var requestParams = new PutItemRequest
        {
            TableName = "restaurants",
            Item = new Dictionary<string, AttributeValue>
        {
            { "id", new AttributeValue(restaurant.Id.ToString()) },
            { "name", new AttributeValue(restaurant.Name) },
            { "address", new AttributeValue(restaurant.Address) },
            { "description", new AttributeValue(restaurant.Description) },
            { "hours", new AttributeValue(restaurant.Hours) },
            { "averagerating", new AttributeValue(restaurant.AverageRating.ToString()) }
        }
        };

        try
        {
            await _dynamoDbClient.PutItemAsync(requestParams);
            return new APIGatewayProxyResponse
            {
                StatusCode = 201,
                Body = "Restaurant created successfully"
            };
        }
        catch (Exception ex)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = $"Error creating restaurant: {ex.Message}"
            };
        }
    }
    private async Task<APIGatewayProxyResponse> UpdateRestaurant(APIGatewayProxyRequest request, ILambdaContext context)
    {
        // Update a restaurant in DynamoDB
        var restaurant = JsonConvert.DeserializeObject<Restaurant>(request.Body);

        var requestParams = new UpdateItemRequest
        {
            TableName = "restaurants",
            Key = new Dictionary<string, AttributeValue>
        {
            { "id", new AttributeValue(restaurant.Id.ToString()) }
        },
            UpdateExpression = "set #name = :name, #address = :address, #description = :description,#hours = :hours,#averagerating = :averagerating",
            ExpressionAttributeNames = new Dictionary<string, string>
        {
            { "#name", "name" },
            { "#address", "address" },
            { "#description", "description" },
            { "#hours", "hours" },
            { "#averagerating", "averagerating" }
        },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
        {
            { ":name", new AttributeValue(restaurant.Name) },
            { ":address", new AttributeValue(restaurant.Address) },
            { ":description", new AttributeValue(restaurant.Description) },
            { ":hours", new AttributeValue(restaurant.Hours) },
            { ":averagerating", new AttributeValue(restaurant.AverageRating.ToString()) }
        }
        };

        try
        {
            await _dynamoDbClient.UpdateItemAsync(requestParams);
            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = "Restaurant updated successfully"
            };
        }
        catch (Exception ex)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = $"Error updating restaurant: {ex.Message}"
            };
        }
    }

    private async Task<APIGatewayProxyResponse> DeleteRestaurant(APIGatewayProxyRequest request, ILambdaContext context)
    {
        // Delete a restaurant from DynamoDB
        var restaurantId = request.PathParameters["id"];

        var requestParams = new DeleteItemRequest
        {
            TableName = "restaurants",
            Key = new Dictionary<string, AttributeValue>
        {
            { "id", new AttributeValue(restaurantId) }
        }
        };

        try
        {
            await _dynamoDbClient.DeleteItemAsync(requestParams);
            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = "Restaurant deleted successfully"
            };
        }
        catch (Exception ex)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = $"Error deleting restaurant: {ex.Message}"
            };
        }
    }
}
