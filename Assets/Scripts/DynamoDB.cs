using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System.Collections.Generic;
using UnityEngine;
using Amazon;
using Amazon.Runtime;
using Amazon.DynamoDBv2.DataModel;
using System.Threading.Tasks;

public class DynamoDB : MonoBehaviour {
    public static DynamoDB Instance;
    public string username;
    public int playerID;
    AmazonDynamoDBClient client;
    public string accessKeyId;
    public string secretAccessKey;
    private static readonly RegionEndpoint RegionEndpoint = RegionEndpoint.USEast1; // Adjust server region
    DynamoDBContext Context;

    void Awake(){
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }
        var credentials = new BasicAWSCredentials(accessKeyId, secretAccessKey);
        client = new AmazonDynamoDBClient(credentials, RegionEndpoint);
        Context = new DynamoDBContext(client);
    }
    
    public async Task<UserStats> GetById(int userId)
    {
        var user = await Context.LoadAsync<UserStats>(userId);
        return user;
    }
    
    public async Task<List<UserStats>> GetAllUsers()
    {
        var user = await Context.ScanAsync<UserStats>(default).GetRemainingAsync();
        return user;
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