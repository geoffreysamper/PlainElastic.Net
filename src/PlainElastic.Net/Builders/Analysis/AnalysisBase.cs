﻿using System;
using System.Collections.Generic;
using System.Linq;
using PlainElastic.Net.Builders;
using PlainElastic.Net.Utils;

namespace PlainElastic.Net.IndexSettings
{
    public abstract class AnalysisBase<TPart> : IJsonConvertible where TPart : AnalysisBase<TPart>
    {
        private readonly List<string> jsonParts = new List<string>();


        public IEnumerable<string> JsonParts
        {
            get { return jsonParts; }
        }


        /// <summary>
        /// Adds a custom part to analisys config.
        /// </summary>
        public TPart CustomPart(string partFormat, params string[] args)
        {
            if (partFormat.IsNullOrEmpty())
                return (TPart)this;

            var json = partFormat.F(args);
            AddJsonPart(json);

            return (TPart)this;
        }


        protected void RegisterJsonPart(string jsonPart, params string[] args)
        {
            if (jsonPart.IsNullOrEmpty())
                return;

            var json = jsonPart.AltQuoteF(args);
            AddJsonPart(json);
        }

        protected void RegisterJsonPartExpression<TJsonPart, TResultJsonPart>(Func<TJsonPart, TResultJsonPart> partExpression)
            where TJsonPart : new()
            where TResultJsonPart : IJsonConvertible
        {
            if (partExpression == null)
                return;

            var inputInstance = new TJsonPart();
            var resultPart = partExpression.Invoke(inputInstance);

            var json = resultPart.ToJson();

            AddJsonPart(json);
        }

        protected void RegisterJsonStringsProperty(string name, IEnumerable<string> values)
        {
            var valuesJson = values.Where(v => !v.IsNullOrEmpty()).Quotate().JoinWithComma();
            RegisterJsonPart("{0}: [ {1} ]", name.Quotate(), valuesJson);
        }

        protected static Func<TComponent, TComponent> SpecifyComponentName<TComponent>(Func<TComponent, TComponent> component, Func<TComponent, string> name) where TComponent : AnalysisComponentBase<TComponent>
        {
            return obj =>
                       {
                           var componentPart = component(obj);
                           return componentPart.Name(name(componentPart));
                       };
        }

        protected static Func<TComponent, TComponent> SpecifyComponentName<TComponent>(Func<TComponent, TComponent> component, string name) where TComponent : AnalysisComponentBase<TComponent>
        {
            if (component == null)
                return obj => obj.Name(name);

            return SpecifyComponentName(component, _ => name);
        }

        protected abstract string ApplyJsonTemplate(string body);


        private void AddJsonPart(string json)
        {
            if (json.IsNullOrEmpty())
                return;

            jsonParts.Add(json);
        }


        string IJsonConvertible.ToJson()
        {
            var body = JsonParts.JoinWithComma();
            return ApplyJsonTemplate(body);
        }


        public override string ToString()
        {
            return ((IJsonConvertible)this).ToJson();
        }
    }
}