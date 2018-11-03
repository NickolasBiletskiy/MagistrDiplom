﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using WriteRoutersToXML.Helpers;
using WriteRoutersToXML.Models.Interfaces;

namespace WriteRoutersToXML.Models
{
    [Serializable]
    public class Router : IDeserializable
    {

        #region Fields

        public string Name { get; set; }

        public Interface[] Interfaces { get; set; }

        public bool IsActive { get; set; }

        #endregion

        #region cstor

        public Router()
        {

        }

        public Router(string name, int numberOfInterfaces)
        {
            Name = name;

            IsActive = true;

            Interfaces = new Interface[numberOfInterfaces];
            for (var i = 0; i < numberOfInterfaces; i++)
            {
                Interfaces[i] = new Interface(this, i);
            }
        }

        #endregion

        #region Public methods

        public void OnDeserializing()
        {
            foreach (Interface inter in Interfaces)
            {
                inter.Router = this;
                //inter.OnDeserializing();
            }

        }

        public void ConnectTo(Router anotherRouter)
        {
            Interface interFaceFrom = GetFirstFreeInterface();
            if (interFaceFrom == null) throw new Exception($"There no free interfaces in {Name}");
            Interface interfaceTo = anotherRouter.GetFirstFreeInterface();
            if (interfaceTo == null) throw new Exception($"There no free interfaces in {interfaceTo.Router.Name}");

            interFaceFrom.CreateConnection(interfaceTo);

        }

        public Interface GetFirstFreeInterface()
        {
            return Interfaces.FirstOrDefault(x => !x.IsConnected);
        }

        public string WriteConnectionsToConsole()
        {
            StringBuilder result = new StringBuilder(this.Name);
            result.Append("\n");
            Interfaces.Where(x => x.IsConnected).ToList().ForEach((inter) =>
            {
                string anotherRouterName = (inter.Link.Interface1 == inter) ? inter.Link.Interface2.Router.Name : inter.Link.Interface1.Router.Name;
                result.Append($"Connection to {anotherRouterName} using {inter.FullName} \n");
                //result.Append("Connection to");
            });
            Console.WriteLine(result.ToString());
            return result.ToString();
        }

        #endregion
    }
}