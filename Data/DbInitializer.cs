using SecureGate.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace SecureGate.Web.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            if (context.Students.Any()) return;

            // 1. CAMERA GROUPS
            var cameraGroups = new CameraGroup[]
            {
                new() { Name = "Kirish nuqtalari" },
                new() { Name = "Koridorlar" },
                new() { Name = "Sinflar" },
                new() { Name = "Tashqi hudud" },
                new() { Name = "Avtoturargoh" }
            };
            context.CameraGroups.AddRange(cameraGroups);
            context.SaveChanges();

            // 2. CAMERAS
            var cameras = new Camera[]
            {
                new() { CameraCode = "CAM-01", Name = "Asosiy kirish — janubiy", IpAddress = "192.168.1.10", Protocol = CameraProtocol.RTSP, Quality = VideoQuality.UHD4K, Status = CameraStatus.Online, FaceRecognitionEnabled = true, CameraGroupId = cameraGroups[0].Id },
                new() { CameraCode = "CAM-02", Name = "Asosiy kirish — shimoliy", IpAddress = "192.168.1.11", Protocol = CameraProtocol.RTSP, Quality = VideoQuality.FullHD, Status = CameraStatus.Online, CameraGroupId = cameraGroups[0].Id },
                new() { CameraCode = "CAM-15", Name = "3-qavat g'arbiy", IpAddress = "192.168.1.24", Quality = VideoQuality.FullHD, Status = CameraStatus.Offline, CameraGroupId = cameraGroups[1].Id },
                new() { CameraCode = "CAM-07", Name = "Kutubxona kirishi", IpAddress = "192.168.1.16", Quality = VideoQuality.UHD4K, Status = CameraStatus.Online, FaceRecognitionEnabled = true, CameraGroupId = cameraGroups[0].Id }
            };
            context.Cameras.AddRange(cameras);
            context.SaveChanges();

            // 3. TURNSTILES
            var turnstiles = new Turnstile[]
            {
                new() { Name = "Asosiy kirish — chap", Location = "1-qavat, janubiy darvoza", IpAddress = "192.168.2.10", Status = TurnstileStatus.Online, TodayPassCount = 412, LinkedCameraId = cameras[0].Id },
                new() { Name = "Asosiy kirish — o'ng", Location = "1-qavat, janubiy darvoza", IpAddress = "192.168.2.11", Status = TurnstileStatus.Online, TodayPassCount = 387, LinkedCameraId = cameras[1].Id }
            };
            context.Turnstiles.AddRange(turnstiles);
            context.SaveChanges();

     

           

            // 6. STUDENTS
            var students = new Users[]
            {
                new() { FirstName = "Alisher", LastName = "Normatov", StudentId = "S-1001", Phone = "+998901234567", Gender = Gender.Male},
                new() { FirstName = "Karim", LastName = "Yo'ldoshev", StudentId = "S-1002", Phone = "+998915678901", Gender = Gender.Male, Status = StudentStatus.Blocked }
            };
            context.Students.AddRange(students);
            context.SaveChanges();

            // 7. FACE DATA SEED (Student va Teacher-dan keyin joylashtirildi)
            if (!context.FaceData.Any())
            {
                var faceSamples = new FaceData[]
                {
                    new() {
                        StudentId = students[0].Id,
                        ImagePath = "/uploads/faces/alisher.jpg",
                        FaceEncoding = "base64_encoded_vector_data_here...",
                        ConfidenceLevel = FaceConfidenceLevel.High
                    },
                    new() {
                        ImagePath = "/uploads/faces/nodira.jpg",
                        FaceEncoding = "base64_encoded_vector_data_here...",
                        ConfidenceLevel = FaceConfidenceLevel.High
                    }
                };
                context.FaceData.AddRange(faceSamples);
            }

            // 8. ACCESS LOGS
            var logs = new AccessLog[]
            {
                new() { StudentId = students[0].Id, TurnstileId = turnstiles[0].Id, CameraId = cameras[0].Id, Method = AccessMethod.Face, Result = AccessResult.Granted, FaceConfidence = 98.5, Timestamp = DateTime.UtcNow },
                new() { StudentId = students[1].Id, TurnstileId = turnstiles[0].Id, CameraId = cameras[0].Id, Method = AccessMethod.Card, Result = AccessResult.Denied, Details = "Bloklangan", Timestamp = DateTime.UtcNow.AddMinutes(-10) }
            };
            context.AccessLogs.AddRange(logs);

            // 9. SETTINGS
            var settings = new Setting[]
            {
                new() { Key = "FaceRecognitionEnabled", Value = "true", Description = "Avtomatik yuzni tanish", Type = SettingType.Boolean },
                new() { Key = "RecordingRetention", Value = "30", Description = "Yozuvlar muddati (kun)", Type = SettingType.Select }
            };
            context.Settings.AddRange(settings);

            context.SaveChanges();
        }
    }
}