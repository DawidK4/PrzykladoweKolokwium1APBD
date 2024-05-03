using System.Data;
using System.Data.SqlClient;
using Kolokwium1.DTOs.Group;

public interface IDbService
{
    Task<GetGroupDTO?> GetGroupDetailsByIdAsync(int id);
    Task<bool> RemoveStudentByIdAsync(int id);
}

public class DbService(IConfiguration configuration) : IDbService
{
    private readonly string _connectionString =
        @"Data Source=DAWID\SQLEXPRESS;Initial Catalog=master;Integrated Security=True";

    private async Task<SqlConnection> GetConnection()
    {
        var connection = new SqlConnection(_connectionString);
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        return connection;
    }

    public async Task<GetGroupDTO?> GetGroupDetailsByIdAsync(int id)
    {
        await using var connection = await GetConnection();
        var getGroupCommand = new SqlCommand();
        getGroupCommand.Connection = connection;
        getGroupCommand.CommandText = """
                                      SELECT g.ID, Nam, s.ID
                                      FROM Groups g left join GroupAssignments ga
                                      on g.ID = ga.Groups_ID
                                      left join Students s
                                      on s.ID = ga.Students_ID
                                      WHERE g.ID = @id
                                      """;
        getGroupCommand.Parameters.AddWithValue("@id", id);
        var reader = await getGroupCommand.ExecuteReaderAsync();

        if (!reader.HasRows) return null;
        await reader.ReadAsync();

        var result = new GetGroupDTO(reader.GetInt32(0), reader.GetString(1),
            !await reader.IsDBNullAsync(2) ? [reader.GetInt32(2)] : []);

        while (await reader.ReadAsync())
        {
            result.students.Add(reader.GetInt32(2));
        }

        return result;
    }

    public async Task<bool> RemoveStudentByIdAsync(int id)
    {
        await using var connection = await GetConnection();
        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            var command1 = new SqlCommand();
            command1.Connection = connection;
            command1.CommandText = """
                                   DELETE FROM GroupAssignments
                                          WHERE Students_ID = @id;
                                   """;
            command1.Transaction = (SqlTransaction)transaction;
            command1.Parameters.AddWithValue("@id", id);
            await command1.ExecuteNonQueryAsync();

            var command2 = new SqlCommand();
            command2.Connection = connection;
            command2.CommandText = """
                                   DELETE FROM Students
                                          WHERE ID = @id;
                                   """;
            command2.Transaction = (SqlTransaction)transaction;
            command2.Parameters.AddWithValue("@id", id);
            var affectedRows = await command2.ExecuteNonQueryAsync();

            await transaction.CommitAsync();

            return affectedRows != 0;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }

    }
}