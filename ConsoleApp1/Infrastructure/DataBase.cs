using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using Organizer.Application;

namespace Organizer.Infrastructure
{
    public class DataBase : IDataBase
    {
        private MySqlConnection connection = new MySqlConnection("server=localhost;port=3306;username=root;password=root;database=organizerbase");

        public DataBase()
        {
            OpenConnectoin();
        }

        public void OpenConnectoin()
        {
            if (connection.State == System.Data.ConnectionState.Closed)
                connection.Open();
        }

        public void CloseConnectoin()
        {
            if (connection.State == System.Data.ConnectionState.Open)
                connection.Close();
        }

        public bool TryGetData(int userId, int dataTypeId, int dataSubTypeId, DateTime time, out byte[] data)
        {
            MySqlCommand command = new MySqlCommand("SELECT * FROM `orgonizer` WHERE `user_id` = @uI AND `data_type_id` = @dTI AND `data_sub_type_id` = @dSTI AND `date_time` = @t", connection);
            command.Parameters.Add("@uI", MySqlDbType.Int32).Value = userId;
            command.Parameters.Add("@dTI", MySqlDbType.Int32).Value = dataTypeId;
            command.Parameters.Add("@dSTI", MySqlDbType.Int32).Value = dataSubTypeId;
            command.Parameters.Add("@t", MySqlDbType.DateTime).Value = time;

            bool result = false;
            string dataStr = null;

            using (DbDataReader dataReader = command.ExecuteReader())
            {
                if (dataReader.HasRows)
                {
                    result = true;

                    while (dataReader.Read())
                    {
                        var dataIndex = dataReader.GetOrdinal("data");
                        dataStr = dataReader.GetString(dataIndex);
                    }
                }
            }
            if (dataStr != null)
            {
                data = ParseStringToByte(dataStr);


            }
            else
                data = null;
            return result;
        }

        public void SaveData(int userId, int dataTypeId, int dataSubTypeId, DateTime time, byte[] data)
        {
            byte[] temp;
            if (!TryGetData(userId, dataTypeId, dataSubTypeId, time, out temp))
            {
                MySqlCommand command = new MySqlCommand("INSERT INTO `orgonizer` (`user_id`, `data_type_id`, `data_sub_type_id`, `date_time`, `data`) VALUES (@uI, @dTI, @dSTI, @t, @d)", connection);
                command.Parameters.Add("@uI", MySqlDbType.Int32).Value = userId;
                command.Parameters.Add("@dTI", MySqlDbType.Int32).Value = dataTypeId;
                command.Parameters.Add("@dSTI", MySqlDbType.Int32).Value = dataSubTypeId;
                command.Parameters.Add("@t", MySqlDbType.DateTime).Value = time;
                command.Parameters.Add("@d", MySqlDbType.Text).Value = BitConverter.ToString(data); //Encoding.UTF8.GetString(data);

                command.ExecuteNonQuery();
            }
            else
            {
                MySqlCommand command = new MySqlCommand("UPDATE `orgonizer` SET  `data` = @d  WHERE `user_id` = @uI AND `data_type_id` = @dTI AND `data_sub_type_id` = @dSTI AND `date_time` = @t", connection);
                command.Parameters.Add("@uI", MySqlDbType.Int32).Value = userId;
                command.Parameters.Add("@dTI", MySqlDbType.Int32).Value = dataTypeId;
                command.Parameters.Add("@dSTI", MySqlDbType.Int32).Value = dataSubTypeId;
                command.Parameters.Add("@t", MySqlDbType.DateTime).Value = time;
                command.Parameters.Add("@d", MySqlDbType.Text).Value = BitConverter.ToString(data);

                command.ExecuteNonQuery();
            }
        }

        public void RemoveData(int userId, int dataTypeId, int dataSubTypeId, DateTime time)
        {
            MySqlCommand command = new MySqlCommand("DELETE FROM `orgonizer` WHERE `user_id` = @uI AND `data_type_id` = @dTI AND `data_sub_type_id` = @dSTI AND `date_time` = @t", connection);
            command.Parameters.Add("@uI", MySqlDbType.Int32).Value = userId;
            command.Parameters.Add("@dTI", MySqlDbType.Int32).Value = dataTypeId;
            command.Parameters.Add("@dSTI", MySqlDbType.Int32).Value = dataSubTypeId;
            command.Parameters.Add("@t", MySqlDbType.DateTime).Value = time;

            command.ExecuteNonQuery();
        }

        private byte[] ParseStringToByte(string dataStr)
        {
            var temp = dataStr.Split('-');
            var data = new List<byte>();

            foreach (var bit in temp)
                data.Add((byte)Convert.ToInt32(bit, 16));

            return data.ToArray();

        }

        ~DataBase()
        {
            CloseConnectoin();
        }
    }
}
