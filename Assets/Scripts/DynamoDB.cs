using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System.Collections.Generic;
using UnityEngine;
using Amazon;
using Amazon.Runtime;
using Amazon.DynamoDBv2.DataModel;
using System.Threading.Tasks;

public class DynamoDB : MonoBehaviour {
    AmazonDynamoDBClient client;
    public AmazonBedrockConnection awsBedrockConnection;
    private static readonly RegionEndpoint RegionEndpoint = RegionEndpoint.USEast1; // Adjust server region
    DynamoDBContext Context;

    void Awake(){
        var credentials = new BasicAWSCredentials(awsBedrockConnection.accessKeyId, awsBedrockConnection.secretAccessKey);
        client = new AmazonDynamoDBClient(credentials, RegionEndpoint);
        Context = new DynamoDBContext(client);
    }
    
    public async Task<bool> GetById(int userId)
    {
        var user = await Context.LoadAsync<UserStats>(userId);
        Debug.Log(user.username);
        return (user != null);
    }
    
    public async Task<bool> GetAllUsers()
    {
        var user = await Context.ScanAsync<UserStats>(default).GetRemainingAsync();
        return (user != null);
    }
    
    public async Task<bool> CreateAndUpdateUser(UserStats userRequest)
    {
        var user = await Context.LoadAsync<UserStats>(userRequest.id);
        if (user != null) return false;
        await Context.SaveAsync(userRequest);
        return true;
    }
    
    public async Task<bool> DeleteUser(int userId)
    {
        var user = await Context.LoadAsync<UserStats>(userId);
        if (user == null) return false;
        await Context.DeleteAsync(user);
        return true;
    }
}