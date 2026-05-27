using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Exceptions.Sample;
using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories.EFCore
{
    public class EFCoreSampleRepository : ISampleRepository
    {
        private readonly IDatabaseContext _context;

        public EFCoreSampleRepository(IDatabaseContext context)
        {
            _context = context;
        }

        public async Task Add(Sample type)
        {
            try
            {
                await _context.Samples.AddAsync(type);
                await _context.SaveChangesAsync(new CancellationToken());
            }
            catch (Exception e)
            {
                throw new SampleAddFailedException(e.ToString()); 
            }
        }

        public async Task Delete(Sample type)
        {
            try
            {
                _context.Samples.Remove(type);

                await _context.SaveChangesAsync(new CancellationToken());
            }
            catch (Exception e)
            {
                throw new SampleDeleteFailedException(e.ToString());
            }
        }

        public async Task Update(Sample type)
        {
            try
            {
                _context.Samples.Update(type);

                await _context.SaveChangesAsync(new CancellationToken());
            }
            catch (Exception e)
            {
                throw new SampleUpdateFailedException(e.ToString());
            }
        }

        public async Task<List<Sample>> GetList()
        {
            return await _context.Samples.ToListAsync();
        }

        public async Task<Sample> GetById(int id)
        {
            return await _context.Samples.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Sample> GetByName(string name)
        {
            return await _context.Samples.FirstOrDefaultAsync(x => x.Name == name);
        }
    }
}
