using System;

namespace IoTPlatform.Domain
{
    public sealed record SerialNumber
    {
      public string Value { get; }

        private SerialNumber(string value)
        {
            Value = value;
        }

        public static SerialNumber Parse(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                throw new ArgumentException("Serial number cannot be empty.");
            }

            if (raw.Length != 7)
            {
                throw new ArgumentException("Serial number must be 7 characters.");
            }

            if (!raw.StartsWith("INT-") && !raw.StartsWith("CAM-"))
            {
                throw new ArgumentException("Serial number must start with INT- or CAM-.");
            }

            for (int i = 4; i < 7; i++)
            {
                if (raw[i] <  '0' || raw[i] > '9')
                {
                    throw new ArgumentException("The last 3 characters must be digits.");
                }
            }

            return new SerialNumber(raw);
        }

    }
}
