using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CityInfo.API.Models;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
    [Route("api/cities")]
    public class PointsOfInterestController : Controller
    {
        [HttpGet("{cityId}/PointsOfInterest")]
        public IActionResult GetPointsOfInterest(int cityId)
        {
            const string brokerList = "mybroker,mybroker2";
            const string topicName = "mytopic";

            var config = new Dictionary<string, object> { { "bootstrap.servers", brokerList } };

            using (var producer = new Producer<Null, string>(config, null, new StringSerializer(Encoding.UTF8)))
            {
                const string text = "my message";
                var deliveryReport = producer.ProduceAsync(topicName, null, text);
                var result = deliveryReport.Result;
                //deliveryReport.ContinueWith(task =>
                //{

                //});

                // Tasks are not waited on synchronously (ContinueWith is not synchronous),
                // so it's possible they may still in progress here.
                producer.Flush();
            }

            var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

            if (city == null)
                return NotFound();

            return Ok(city.PointsOfInterest);
        }

        [HttpGet("{cityId}/PointsOfInterest/{id}", Name = "GetPointsOfInterest")]
        public IActionResult GetPointsOfInterest(int cityId, int id)
        {
            var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

            if (city == null)
                return NotFound();

            var pointsOfInterest = city.PointsOfInterest.FirstOrDefault(p => p.Id == id);

            if (pointsOfInterest == null)
                return NotFound();

            return Ok(pointsOfInterest);
        }

        [HttpPost("{cityId}/PointsOfInterest")]
        public IActionResult CreatePointsOfInterest(int cityId,
            [FromBody] PointsOfInterestForCreationDto pointsOfInterest)
        {
            if (pointsOfInterest == null)
                return BadRequest();

            if (pointsOfInterest.Description == pointsOfInterest.Name)
                ModelState.AddModelError("Description", "The provided description should be different from the name.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);
            if (city == null)
                return NotFound();

            var maxPointsOfInterest = CitiesDataStore.Current.Cities.SelectMany(c => c.PointsOfInterest).Max(p => p.Id);
            var finalPointsOfInterest = new PointsOfInterestDto
            {
                Id = ++maxPointsOfInterest,
                Name = pointsOfInterest.Name,
                Description = pointsOfInterest.Description
            };

            city.PointsOfInterest.Add(finalPointsOfInterest);

            return CreatedAtRoute("GetPointsOfInterest", new {cityId = cityId, id = finalPointsOfInterest.Id},
                finalPointsOfInterest);
        }

        [HttpPut("{cityId}/PointsOfInterest/{id}")]
        public IActionResult UpdatePointsOfInterest(int cityId, int id,
            [FromBody] PointsOfInterestForCreationDto pointsOfInterest)
        {
            if (pointsOfInterest == null)
                return BadRequest();

            if (pointsOfInterest.Description == pointsOfInterest.Name)
                ModelState.AddModelError("Description", "The provided description should be different from the name.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);
            if (city == null)
                return NotFound();

            var pointOfInterestFromStore = city.PointsOfInterest.FirstOrDefault(p => p.Id == id);
            if (pointOfInterestFromStore == null)
                return NotFound();

            pointOfInterestFromStore.Name = pointsOfInterest.Name;
            pointOfInterestFromStore.Description = pointsOfInterest.Description;

            return NoContent();
        }

        [HttpPatch("{cityId}/PointsOfInterest/{id}")]
        public IActionResult PartiallyUpdatePointOfInterest(int cityId, int id,
            [FromBody] JsonPatchDocument<PointsOfInterestForUpdateDto> patchDoc)
        {
            if (patchDoc == null)
                return BadRequest();

            var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);
            if (city == null)
                return NotFound();

            var pointOfInterestFromStore = city.PointsOfInterest.FirstOrDefault(p => p.Id == id);
            if (pointOfInterestFromStore == null)
                return NotFound();

            var pointofInterestToPatch = new PointsOfInterestForUpdateDto
            {
                Name = pointOfInterestFromStore.Name,
                Description = pointOfInterestFromStore.Description

            };

            patchDoc.ApplyTo(pointofInterestToPatch, ModelState);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            pointOfInterestFromStore.Name = pointofInterestToPatch.Name;
            pointOfInterestFromStore.Description = pointofInterestToPatch.Description;

            return NoContent();
        }
    }
}
