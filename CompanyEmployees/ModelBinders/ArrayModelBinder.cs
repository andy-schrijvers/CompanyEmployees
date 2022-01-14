using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CompanyEmployees.ModelBinders
{
    public class ArrayModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {

            // check if the type is IEnumarable
            if (!bindingContext.ModelMetadata.IsEnumerableType)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }


            // incoming value
            var providedValue = bindingContext.ValueProvider
                .GetValue(bindingContext.ModelName)
                .ToString();

            //if the incoming value is null return success(null)
            if (string.IsNullOrEmpty(providedValue))
            {
                bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            try
            {
                // extract the generic type
                var genericType = bindingContext.ModelType.GetTypeInfo().GenericTypeArguments[0];

                var converter = TypeDescriptor.GetConverter(genericType);

                var objectArray = providedValue.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => converter.ConvertFromString(x.Trim())).ToArray();

                var guidArray = Array.CreateInstance(genericType, objectArray.Length);
                objectArray.CopyTo(guidArray, 0);
                bindingContext.Model = guidArray;

                bindingContext.Result = ModelBindingResult.Success(bindingContext.Model);
            }

            catch (Exception)
            {
                bindingContext.Result = ModelBindingResult.Failed();

            }


            return Task.CompletedTask;
        }


    }
}
