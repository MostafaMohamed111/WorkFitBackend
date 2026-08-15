$connStr = "Server=localhost\MSSQLSERVER4;Database=New_db57635;Encrypt=False;Trusted_Connection=True;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
$conn.Open()

$sql = @"
DELETE FROM [ProjectManagement].[project_activity_logs] WHERE ProjectId NOT IN (SELECT Id FROM [ProjectManagement].[projects]);
DELETE FROM [ProjectManagement].[task_github] WHERE TaskId NOT IN (SELECT Id FROM [ProjectManagement].[tasks]);
DELETE FROM [ProjectManagement].[tasks] WHERE ProjectId NOT IN (SELECT Id FROM [ProjectManagement].[projects]);
DELETE FROM [ProjectManagement].[ProjectMembers] WHERE ProjectId NOT IN (SELECT Id FROM [ProjectManagement].[projects]);
DELETE FROM [ProjectManagement].[ProjectRequiredSkills] WHERE ProjectId NOT IN (SELECT Id FROM [ProjectManagement].[projects]);
IF OBJECT_ID('workflow.DeveloperInvitations', 'U') IS NOT NULL
BEGIN
    DELETE FROM [workflow].[DeveloperInvitations] WHERE ProjectId NOT IN (SELECT Id FROM [ProjectManagement].[projects]);
END
"@

$cmd = $conn.CreateCommand()
$cmd.CommandText = $sql
$affected = $cmd.ExecuteNonQuery()
Write-Host "Database orphan cleanup completed successfully! Total orphan rows purged: $affected"

$conn.Close()
