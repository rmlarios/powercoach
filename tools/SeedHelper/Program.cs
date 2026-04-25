using BCrypt.Net;
string hash = BCrypt.Net.BCrypt.HashPassword("Admin@123456", 11);
Console.WriteLine(hash);
