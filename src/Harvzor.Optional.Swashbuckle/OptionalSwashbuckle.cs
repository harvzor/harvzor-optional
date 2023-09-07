using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Harvzor.Optional.Swashbuckle;

public static class OptionalSwashbuckle
{
    public static SwaggerGenOptions FixOptionalMappings(this SwaggerGenOptions options, params Assembly[] assemblies)
    {
        options.UseAllOfToExtendReferenceSchemas();
        options.DocumentFilter<OptionalDocumentFilter>();

        return options;
    }

    private class OptionalDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach (var schema in swaggerDoc.Components.Schemas)
            {
                RemoveOptionalLevelFromProperties(schema.Value, context);
            }

            RemoveOptionalSchemas(swaggerDoc);
        }

        private void RemoveOptionalLevelFromProperties(OpenApiSchema schema, DocumentFilterContext context)
        {
            if (schema.Properties == null)
            {
                return;
            }

            foreach (var property in schema.Properties.ToList())
            {
                if (property.Value.AllOf != null)
                {
                    // swashbuckle uses allOf for references, so that custom coments can be included.
                    for (int i = 0; i < property.Value.AllOf.Count; i++)
                    {
                        var currentSchema = property.Value.AllOf[i];
                        if (IsReferenceToOptional(currentSchema.Reference.Id, context.SchemaRepository))
                        {
                            var optionalSchema = context.SchemaRepository.Schemas[currentSchema.Reference.Id];

                            if (!optionalSchema.Properties.TryGetValue("value", out var valueSchema))
                            {
                                throw new InvalidOperationException("Optional schema must have a value property.");
                            }

                            if (valueSchema.Reference != null)
                            {
                                // if the value of optional is a reference (i.e. a complex type), then just use it as all off.
                                property.Value.AllOf[i] = valueSchema;
                            }
                            else
                            {
                                // this is e.g. Optional<string>. We can't use AllOf here, so we must replace the whole property.
                                schema.Properties[property.Key] = valueSchema;
                            }
                        }
                    }
                }
            }
        }

        private static bool IsReferenceToOptional(string referenceId, SchemaRepository schemaRepository)
        {
            var referencedSchema = schemaRepository.Schemas.First(x => x.Key == referenceId);
            return IsOptionalSchema(referencedSchema);
        }

        private static bool IsOptionalSchema(KeyValuePair<string, OpenApiSchema> referencedSchema)
        {
            return referencedSchema.Key.EndsWith("Optional") &&
                referencedSchema.Value.Properties.Count() == 2 &&
                referencedSchema.Value.Properties.Any(x => x.Key == "value") &&
                referencedSchema.Value.Properties.Any(x => x.Key == "hasValue");
        }

        private void RemoveOptionalSchemas(OpenApiDocument swaggerDoc)
        {
            swaggerDoc.Components.Schemas
                .Where(IsOptionalSchema)
                .ToList()
                .ForEach(schema => swaggerDoc.Components.Schemas.Remove(schema));
        }
    }
}
