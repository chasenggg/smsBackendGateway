using Microsoft.EntityFrameworkCore;


namespace smsBackendGateway.Models
{
    public class SmsContext : DbContext
    {

        public SmsContext(DbContextOptions<SmsContext> options) : base(options) 
        { 
        
        }
        
        public DbSet<Sms> sms { get; set;}
    }
}
