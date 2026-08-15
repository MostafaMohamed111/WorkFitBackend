$connStr = "Server=localhost\MSSQLSERVER4;Database=New_db57635;Encrypt=False;Trusted_Connection=True;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
$conn.Open()

$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT TABLE_SCHEMA, TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'"
$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
    Write-Host "$($reader['TABLE_SCHEMA']).$($reader['TABLE_NAME'])"
}
$conn.Close()
