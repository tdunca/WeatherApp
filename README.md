# WeatherAppLab3
This is a school assignment for DevSecOps #C

## Requirements

- **.NET 8.0 or higher**
- **SQL Server LocalDB**
- **CSV-file `TempFuktData.csv`**  

## Placement of CSV-file

The file path for CSV file needs to be added in `Program.cs`:

```csharp
var filePath = @"C:\Users\there\Desktop\TempFuktData.csv";
```

First run:

1. The program reads the CSV-file  
2. Automatically creates the database  
3. Imports data if the database is empty 

##  Installing and running:

### 1. Clone or download project

### 2. Open in Visual Studio

### 3. Insure the NuGet-package is installed  
Visual Studio usually does this automatically when building.

If not, run:

```
dotnet restore
```

### 4. Use correct file path for CSV in Program.cs

```csharp
var filePath = @"C:\Users\there\Desktop\TempFuktData.csv";
```

### 5. Build and run 
`Build > Build Solution` or `Ctrl + F5`

### 6. Use menu options in the console
