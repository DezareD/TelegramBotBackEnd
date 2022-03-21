using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBot.Data;

namespace TelegramBot
{
    public interface IApplicationServices
    {
        Task<Client> FindClientById(long id);
        Task UpdateClient(Client client);
        Task AddClient(Client client);

        Task<Applicant> GetLastNonApplyedApplicant();
        Task UpdateApplicant(Applicant applicant);
        Task AddAplicant(Applicant applicant);
        Task<Applicant> FindApplicantById(int id);

        Task<bool> UserIsSendApplicant(Client client);
        Task<List<long>> GetAllAdminsChatIds();

        Task CreateArtObject(ArtObject obj);
        Task<ArtObject> GetArtObjectById(int id);
        Task<List<ArtObject>> GetAllArtObjects();
        Task DeleteAllObject();
        Task DeleteArtObject(ArtObject obj);
        Task UpdateArtObject(ArtObject obj);
    }

    public class ApplicationServices : IApplicationServices
    {
        private TelegramBotDataBaseConnection _dbContext;

        public ApplicationServices(TelegramBotDataBaseConnection dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Client> FindClientById(long id) => await _dbContext.clients.Where(m => m.UserId == id).FirstOrDefaultAsync();

        public async Task UpdateClient(Client client)
        {
            _dbContext.clients.Update(client);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Applicant> FindApplicantById(int id) => await _dbContext.applicants.Where(m => m.Id == id).FirstOrDefaultAsync();

        public async Task<List<long>> GetAllAdminsChatIds() => await _dbContext.clients.Where(m => m.UniqRoleName == "admin").Select(m => m.ChatId).ToListAsync();

        public async Task AddClient(Client client)
        {
            _dbContext.clients.Add(client);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> UserIsSendApplicant(Client client)
        {
            var applicant = await _dbContext.applicants.Where(m => m.AuthorId == client.Id).Where(m => m.Status == "status_waiting").FirstOrDefaultAsync();

            if (applicant == null) return false;
            else return true;
        }

        public async Task CreateArtObject(ArtObject obj)
        {
            _dbContext.artObjects.Add(obj);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Applicant> GetLastNonApplyedApplicant() => await _dbContext.applicants.Where(m => m.Status == "status_waiting").FirstOrDefaultAsync();

        public async Task UpdateApplicant(Applicant applicant)
        {
            _dbContext.applicants.Update(applicant);
            await _dbContext.SaveChangesAsync();
        }

        public async Task AddAplicant(Applicant applicant)
        {
            _dbContext.applicants.Add(applicant);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<ArtObject> GetArtObjectById(int id) => await _dbContext.artObjects.Where(m => m.Id == id).FirstOrDefaultAsync();

        public async Task<List<ArtObject>> GetAllArtObjects() => await _dbContext.artObjects.ToListAsync();

        public async Task DeleteArtObject(ArtObject obj)
        {
            _dbContext.artObjects.Remove(obj);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAllObject()
        {
            var list = await GetAllArtObjects();
            _dbContext.RemoveRange(list);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateArtObject(ArtObject obj)
        {
            _dbContext.artObjects.Update(obj);
            await _dbContext.SaveChangesAsync();
        }
    }
}
