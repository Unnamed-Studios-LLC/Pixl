using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Foundation;

namespace Pixl.Mac
{
    internal sealed class MacValuesStore : ValuesStore
	{
        private readonly string _plstPath;

        private NSMutableDictionary? _dictionary;

        public MacValuesStore(string productId)
        {
            if (productId is null)
            {
                throw new ArgumentNullException(nameof(productId));
            }

            var libraryPath = NSSearchPath.GetDirectories(NSSearchPathDirectory.LibraryDirectory, NSSearchPathDomain.Local, true).First();
            _plstPath = Path.Combine(libraryPath, "Preferences", $"{productId}.plst");
        }

        public override void Read(Dictionary<string, StoredValue> all)
        {
            if (!File.Exists(_plstPath)) return;
            _dictionary = NSMutableDictionary.FromFile(_plstPath);
            foreach (var (key, value) in _dictionary)
            {
                if (key is not NSString keyString) continue;
                StoredValue? storedValue = value switch
                {
                    NSString valueString => new StoredValue(ValueType.String, valueString),
                    NSDecimalNumber valueDecimal => new StoredValue(ValueType.Float, valueDecimal.FloatValue),
                    NSNumber valueNumber => new StoredValue(ValueType.Int, valueNumber.Int32Value),
                    _ => null
                };
                if (storedValue == null) continue;
                all[keyString] = storedValue.Value;
            }
        }

        public override void Write(Dictionary<string, StoredValue> all, Dictionary<string, StoredValue> edited)
        {
            _dictionary ??= new();
            foreach (var (key, storedValue) in edited)
            {
                switch (storedValue.Type)
                {
                    case ValueType.String:
                        _dictionary[key] = new NSString((string)storedValue.Value);
                        break;
                    case ValueType.Float:
                        _dictionary[key] = new NSDecimalNumber((float)storedValue.Value);
                        break;
                    case ValueType.Int:
                        _dictionary[key] = new NSNumber((int)storedValue.Value);
                        break;
                }
            }
            _dictionary.WriteToUrl(NSUrl.FromFilename(_plstPath), true);
        }
    }
}

