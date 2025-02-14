https://www.thinktecture.com/blazor/authentifizierung-und-autorisierung/

In einer Liste alle Properties ausgeben
Console.WriteLine(args.DeletedRecords);
            List<Appointment> appointments = args.DeletedRecords;
            foreach (Appointment appointment in appointments)
            {
                Console.WriteLine("Appointment:");
                Type appointmentType = typeof(Appointment);
                foreach (PropertyInfo propInfo in appointmentType.GetProperties())
                {
                    Console.WriteLine($"{propInfo.Name}: {propInfo.GetValue(appointment)}");
                }
            }