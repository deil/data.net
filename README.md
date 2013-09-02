data.net
========

Data.NET is a library for loading data from remote sources. With dependencies. In parallel.

Usage
-----

First, define input parameters that can be used to load data:
``` csharp
public enum MyParameterType
{
  UserId,
  Period
}
```

Define data types with required or optional parameters:
``` csharp
public enum MyDataType
{
  [InputParameters((int)MyParameterType.UserId)]
  UserProfile,
  
  [InputParameters((int)MyParameterType.UserId)]
  [OptionalInputParameter((int)MyParameterType.Period)]
  Statistics
}
```

And implement IDataSource interface to actually load defined data types:
``` csharp
public class MyDataSource : Data.Net.IDataSource<MyParameterType, MyDataType>
{
  public object LoadData(MyDataType dataType, IDictionary<MyParameterType, object> parameterValues, ILoadingContext<MyParameterType> context)
  {
    switch (dataType) 
    {
      case MyDataType.UserProfile:
        return LoadUserProfile((int)parameterValues[MyParameterType.UserId]);
        break;
        
      case MyDataType.Statistics:
        int period = 0;
        return LoadStatistics((int)parameterValues[MyParameterType.UserId], parameterValues.TryGetValue(MyParameterType.Statistics, ref period) ? period : (int?)null);
        break;
    }
  }
  
  private UserProfile LoadUserProfile(int profileId) 
  {
    // loads user profile by id
  }
  
  private Statistics[] LoadStatistics(int profileId, int? periodId)
  {
    // loads user statistics for specified profileId
  }
}
```

Just ask Data.Net to load your data:
``` csharp
  var parameters = new Dictionary<MyParameterType, object>() { {MyParameterType.UserId, 1} };
  var data = new DataProvider(new MyDataSource()).LoadData(parameters, MyDataType.UserProfile, MyDataType.Period);
```

Data.Net will use ThreadPool to load all requested data in parallel.
