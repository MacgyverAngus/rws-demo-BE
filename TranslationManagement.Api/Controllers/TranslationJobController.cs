using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using DomainObjects;
using External.ThirdParty.Services;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Repositories;
using TranslationManagement.Api.Controlers;
using TranslationManagement.Api.DataContracts;

namespace TranslationManagement.Api.Controllers
{
    [ApiController]
    [Route("api/jobs")]
    public class TranslationJobController : ControllerBase
    {
       

        private AppDbContext _context;
        private readonly IValidator<CreateTranslationJobDto> _createTranslationJobValidator;
        private readonly ITranslationJobRepository _translationJobRepository;
        private readonly ILogger<TranslatorManagementController> _logger;

        public TranslationJobController(
            IServiceScopeFactory scopeFactory,
            IValidator<CreateTranslationJobDto> createTranslationJobValidator,
            ITranslationJobRepository translationJobRepository,
            ILogger<TranslatorManagementController> logger)
        {
            _context = scopeFactory.CreateScope().ServiceProvider.GetService<AppDbContext>();
            _createTranslationJobValidator = createTranslationJobValidator;
            _translationJobRepository = translationJobRepository;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetJobs()
        {
            return Ok(_translationJobRepository.GetTranslationJobs().Select(MapToDto).ToArray());
        }

        private TranslationJobDto MapToDto(TranslationJob job)
        {
            return new TranslationJobDto
            {
                Id = job.Id,
                CustomerName = job.CustomerName,
                OriginalContent = job.OriginalContent,
                TranslatedContent= job.TranslatedContent,
                Price = job.Price,
                Status = job.Status.ToString()
            };
        }

        const double PricePerCharacter = 0.01;

        private static double GetPrice(string content, double pricePerCharacter)
        {
            return content.Length * pricePerCharacter;
        }

        [HttpPost]
        public IActionResult CreateJob([FromForm] CreateTranslationJobDto job)
        {
            //TODO: global exception handling
            // validate input
            var validationResult = _createTranslationJobValidator.Validate(job);
            if(!validationResult.IsValid)
            {
                //return 400 if not valid
                return (IActionResult)Results.ValidationProblem(validationResult.ToDictionary());
            }

            var newJob = new TranslationJob
            {
                CustomerName = job.CustomerName,
                OriginalContent = job.OriginalContent,
                TranslatedContent = job.TranslatedContent,
                Status = JobStatuses.New,
                Price = GetPrice(job.OriginalContent, PricePerCharacter)
            };
            
            //UnitOfWork pattern
            _translationJobRepository.SaveTranslationJob(newJob);
           ;
            //TODO: isolate 3rd party service
            //TODO: send notifications by some queue to unreliable notification service so its consumed out of this service call
            // e.g. rabbit mq, queue
            if (_translationJobRepository.Save() > 0)
            {
                var notificationSvc = new UnreliableNotificationService();
                while (!notificationSvc.SendNotification("Job created: " + newJob.Id).Result)
                {
                }

                _logger.LogInformation("New job notification sent");
            }

            return Created("uri to translation job resource", newJob);
        }

        [HttpPost("{id}")]
        public IActionResult UpdateJobStatus(int id, [FromForm] UpdateJobStatusDto updateJobStatusDto)
        {
            _logger.LogInformation("Job status update request received: " + updateJobStatusDto.Status + " for job " + id.ToString() + " by translator " + updateJobStatusDto.TranslatorId);
            if (!Enum.IsDefined(typeof(JobStatuses), updateJobStatusDto.Status)) { 
                return BadRequest("invalid status");
            }

            var job = _context.TranslationJobs.Single(j => j.Id == id);

            bool isInvalidStatusChange = (job.Status == JobStatuses.New && (JobStatuses)Enum.Parse(typeof(JobStatuses),updateJobStatusDto.Status) == JobStatuses.Completed) ||
                                         job.Status == JobStatuses.Completed || (JobStatuses)Enum.Parse(typeof(JobStatuses), updateJobStatusDto.Status) == JobStatuses.New;
            if (isInvalidStatusChange)
            {
                return BadRequest("invalid status change");
            }

            job.Status = (JobStatuses)Enum.Parse(typeof(JobStatuses), updateJobStatusDto.Status);
            _context.SaveChanges();
            return Ok();
        }

        [HttpPost("file")]
        public bool CreateJobWithFile(IFormFile file, string customer)
        {
            var reader = new StreamReader(file.OpenReadStream());
            string content;

            if (file.FileName.EndsWith(".txt"))
            {
                content = reader.ReadToEnd();
            }
            else if (file.FileName.EndsWith(".xml"))
            {
                var xdoc = XDocument.Parse(reader.ReadToEnd());
                content = xdoc.Root.Element("Content").Value;
                customer = xdoc.Root.Element("Customer").Value.Trim();
            }
            else
            {
                throw new NotSupportedException("unsupported file");
            }

            var newJob = new TranslationJob()
            {
                OriginalContent = content,
                TranslatedContent = "",
                CustomerName = customer,
                Price = GetPrice(content, PricePerCharacter),
            };

            // TODO: not this way - do not call other controller method
            //return CreateJob(newJob);
            return true;
        }

    }
}