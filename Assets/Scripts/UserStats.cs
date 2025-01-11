using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Amazon.DynamoDBv2.DataModel;

[DynamoDBTable("chiron_leaderboard")]
public class UserStats {
    [DynamoDBHashKey("id")]   // Hash key.
    public int id;
    [DynamoDBProperty("username")]
    public string username;
    [DynamoDBProperty("totalTime")]
    public float totalTime;
}