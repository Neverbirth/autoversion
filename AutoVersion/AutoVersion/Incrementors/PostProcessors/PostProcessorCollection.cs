using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Qreed.Reflection;

namespace AutoVersion.Incrementors.PostProcessors
{
    internal class PostProcessorCollection
    {
        private Dictionary<string, BasePostProcessor> _postProcessors = new Dictionary<string, BasePostProcessor>();

        /// <summary>
        /// Gets the <see cref="BuildVersionIncrement.Incrementors.BaseIncrementor"/> with the specified name.
        /// </summary>
        /// <value>The name of the incrementor.</value>
        public BasePostProcessor this[string name]
        {
            get
            {
                if (_postProcessors.ContainsKey(name))
                    return _postProcessors[name];

                return null;
            }
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>The count.</value>
        public int Count
        {
            get
            {
                return _postProcessors.Keys.Count;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IncrementorCollection"/> class.
        /// </summary>
        public PostProcessorCollection()
        {
            // Add the static null instance to the list. This ensures that property grid references the "correct" null. 
            _postProcessors.Add(BuiltInBasePostProcessor.NoneProcessor.Instance.Name,
                              BuiltInBasePostProcessor.NoneProcessor.Instance);
        }

        /// <summary>
        /// Locates BaseIncrementors in the given assembly and add them to the collection.
        /// </summary>
        /// <param name="asm">The assembly.</param>
        public void AddFrom(Assembly asm)
        {
            List<Type> types = ReflectionHelper.GetTypesThatDeriveFromType(asm, typeof(BasePostProcessor), false, false);

            foreach (Type t in types)
            {
                if (t == typeof(BuiltInBasePostProcessor.NoneProcessor)) // Don't add the null incrementor; this is done in the constructor.
                    continue;

                BasePostProcessor processor = (BasePostProcessor)Activator.CreateInstance(t);

                _postProcessors.Add(processor.Name, processor);
            }
        }

        /// <summary>
        /// Gets the incrementor names.
        /// </summary>
        /// <returns>An array containing all the names of the incrementors.</returns>
        public string[] GetPostProcessorNames()
        {
            string[] ret = new string[Count];

            _postProcessors.Keys.CopyTo(ret, 0);

            return ret;
        }

        /// <summary>
        /// Gets the incrementors.
        /// </summary>
        /// <returns>An array containing the incrementors</returns>
        public BasePostProcessor[] GetPostProcessors()
        {
            BasePostProcessor[] ret = new BasePostProcessor[Count];

            _postProcessors.Values.CopyTo(ret, 0);

            return ret;
        }
    }
}

