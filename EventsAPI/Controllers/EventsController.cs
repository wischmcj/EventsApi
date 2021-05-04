﻿using EventsAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EventsAPI.Controllers
{
    [Route("events")]
    public class EventsController : ControllerBase
    {

         private readonly EventsDataContext _context;


        public EventsController(EventsDataContext context)
        {
            _context = context;
        }

        [HttpGet("{id:int}/participants")] // GET /events/1/partipants
        public async Task<ActionResult> GetPartipantsForEvent(int id)
        {
            var savedEvent = await _context.Events.Where(e => e.Id == id).SingleOrDefaultAsync();
            if (savedEvent == null)
            {
                return NotFound();
            }
            // ??? Run the query again?
            var participants = await _context.Events
                .Where(e => e.Id == id)
                .Select(e => e.Participants)
                .ToListAsync();

            return Ok(participants); // Bad again!

        }



        [HttpPost]
        [ResponseCache(Location =ResponseCacheLocation.Any, Duration = 10)]
        public async Task<ActionResult> AddEvent([FromBody] PostEventRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            else
            {


                var eventToAdd = new Event()
                {
                    Name = request.Name,
                    HostedBy = request.HostedBy,
                    LongDescription = request.LongDescription,
                    StartDateAndTime = (DateTime)request.StartDateAndTime,
                    EndDateAndTime = (DateTime)request.EndDateAndTime
                };
                _context.Events.Add(eventToAdd);
                await _context.SaveChangesAsync();
                /*var url = Url.Action(nameof(EventsController.GetById), nameof(EventsController), new { eventToAdd.Id });
                return Created(url, eventToAdd);*/
                return CreatedAtRoute("get-event-by-id", new { id = eventToAdd.Id }, eventToAdd); // this is a bit wrong.. stay with me.

            }
        }

        [HttpGet]
        
        public async Task<ActionResult> Get([FromQuery] bool showPast = false )
        {
            var details = await _context.Events
                .Where(e => e.EndDateAndTime.Date > DateTime.Now.Date)
                .Select(e => new GetEventsResponseItem(e.Id, e.Name, e.StartDateAndTime, e.EndDateAndTime, e.Participants.Count()))
                    .ToListAsync();

            return Ok(new GetResponse<GetEventsResponseItem>(details));
        }

        [HttpGet("{id:int}",Name="get-event-by-id")]
        public async Task<ActionResult> GetById(int id)
        {
            var details = await _context.Events
                .Where(e => e.EndDateAndTime.Date > DateTime.Now.Date && e.Id == id)
                .Select(e => new GetEventsResponseItem(e.Id, e.Name, e.StartDateAndTime, e.EndDateAndTime, e.Participants.Count()))
                    .ToListAsync();

            return Ok(new GetResponse<GetEventsResponseItem>(details));
        }
    }

    public record GetResponse<T>(IList<T> Data);

    public record GetEventsResponseItem(int Id, string Name, DateTime StartDate, DateTime EndDate, int NumberOfParticipants);

    public record PostEventRequest(
        [Required]
        string Name,
        string LongDescription,
        [Required]
        string HostedBy,
        [Required]
        DateTime? StartDateAndTime,
        [Required]
        DateTime? EndDateAndTime
        );


}
