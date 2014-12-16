﻿namespace uLocate.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;

    using uLocate.Models;
    using uLocate.Persistance;
    using uLocate.Data;
    using Umbraco.Core;
    using Umbraco.Web.WebApi;

    //TODO: Add to back-office App area / secure this

    /// <summary>
    /// uLocate initialization api controller.
    /// </summary>
    public class InitializationApiController : UmbracoApiController
    {
        /// <summary>
        /// Initializes the uLocate Database tables
        /// /Umbraco/Api/InitializationApi/InitDb
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        [AcceptVerbs("GET")]
        public bool InitDb()
        {
            bool Result = uLocate.Data.Helper.InitializeDatabase();

            return Result;
        }

        /// <summary>
        /// Deletes the uLocate Database tables
        /// /Umbraco/Api/InitializationApi/DeleteDb
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        [AcceptVerbs("GET")]
        public bool DeleteDb()
        {
            bool Result = uLocate.Data.Helper.DeleteDatabase();

            return Result;
        }

        /// <summary>
        /// Used for testing (deletes &amp; re-creates database)
        ///  /Umbraco/Api/InitializationApi/ResetDb
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        [AcceptVerbs("GET")]
        public bool ResetDb()
        {
            bool Result = uLocate.Data.Helper.DeleteDatabase();
            if (Result)
            {
                Result = uLocate.Data.Helper.InitializeDatabase();
            }

            return Result;
        }

        /// <summary>
        /// Used for testing
        ///  /Umbraco/Api/InitializationApi/TestPopulateSomeLocationTypes
        /// </summary>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        [AcceptVerbs("GET")]
        public IEnumerable<LocationType> TestPopulateSomeLocationTypes()
        {
            int NewItemId = 0;
            string Msg = "";

            //TEST Add "Business" location type with 2 properties
            var NewItem1 = new LocationType();
            NewItem1.Name = "Business";
            NewItem1.AddProperty("BusinessWebsite", "Business Website URL", uLocate.Constants.DataTypeId.TextBox);
            NewItem1.AddProperty("BusinessHours", "Hours of Operation", uLocate.Constants.DataTypeId.TextBoxMultiple);
            Repositories.LocationTypeRepo.Insert(NewItem1);
            Msg += string.Format("Type '{0}' added. ", NewItem1.Name);

            //TEST Add "Shopping Center" location type with 2 properties
            var NewItem2 = new LocationType();
            NewItem2.Name = "Shopping Center";
            NewItem2.AddProperty("SCName", "Shopping Center Name", uLocate.Constants.DataTypeId.TextBox);
            NewItem2.AddProperty("SCHours", "Hours of Operation", uLocate.Constants.DataTypeId.TextBoxMultiple);
            Repositories.LocationTypeRepo.Insert(NewItem2);
            Msg += string.Format("Type '{0}' added. ", NewItem2.Name);

            //TEST: Return all Location Types
            var Result = Repositories.LocationTypeRepo.GetAll();

            return Result;
        }

        /// <summary>
        /// Used for testing
        ///  /Umbraco/Api/InitializationApi/TestUpdateLocationType
        /// </summary>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        [AcceptVerbs("GET")]
        public IEnumerable<LocationTypeApiController> TestUpdateLocationType()
        {
            int NewItemId = 0;
            string Msg = "";

            //TEST: Update a Location Type
            List<LocationTypeApiController> Result = new List<LocationTypeApiController>();

            //change the data
            var Lt = Repositories.LocationTypeRepo.GetByName("Shopping Center").FirstOrDefault();
            Lt.Name = "Shopping Mall";
            Repositories.LocationTypeRepo.Update(Lt);

            var Prop = Lt.Properties.Where(p => p.Alias == "SCHours").FirstOrDefault();
            if (Prop != null)
            {
                Prop.Alias = "BusinessHours";
                Repositories.LocationTypePropertyRepo.Update(Prop);
            }

            var Prop2 = Lt.Properties.Where(p => p.Alias == "SCName").FirstOrDefault();
            if (Prop2 != null)
            {
                Prop2.Alias = "SMName";
                Prop2.Name = "Shopping Mall Name";
                Repositories.LocationTypePropertyRepo.Update(Prop2);
            }

            //show new data
            Result.Add(Lt);

            return Result;
        }

        /// <summary>
        /// Used for testing
        ///  /Umbraco/Api/InitializationApi/TestDeleteLocationType?LookupName="name"
        /// </summary>
        /// <param name="LookupName">
        /// The Lookup Name.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        [AcceptVerbs("GET")]
        public StatusMessage TestDeleteLocationType(string LookupName = "Business")
        {
            StatusMessage StatusMsg = new StatusMessage();
            StatusMsg.ObjectName = LookupName;

            //TEST Delete a LocationType
            var LookupItem = Repositories.LocationTypeRepo.GetByName(LookupName).FirstOrDefault();
            if (LookupItem != null)
            {
                StatusMsg = Repositories.LocationTypeRepo.Delete(LookupItem);
            }
            else
            {
                StatusMsg.Message = string.Format("'{0}' was not found and can not be deleted.", LookupName);
                StatusMsg.Success = false;
            }

            return StatusMsg;
        }


        /// <summary>
        /// Used for testing
        /// /Umbraco/Api/InitializationApi/Test
        /// </summary>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        [AcceptVerbs("GET")]
        public Location Test()
        {
            int NewItemId = 0;
            string Msg = "";

      

            //TEST: Add a new Location 
            var newLoc = new Location("Test Location");
            Repositories.LocationRepo.Insert(newLoc);

            var Result = Repositories.LocationRepo.GetByKey(newLoc.Key);

            return Result;
        }
    }
}
