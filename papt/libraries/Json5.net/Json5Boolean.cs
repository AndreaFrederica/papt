namespace Json5
{
    public class Json5Boolean : Json5Primitive
    {
        private bool value;

        public Json5Boolean(bool value)
        {
            this.value = value;
        }

        public override Json5Type Type
        {
            get { return Json5Type.Boolean; }
        }

        protected override object Value
        {
            get { return this.value; }
        }

        internal override string ToJson5String(string space, string indent, bool useOneSpaceIndent = false)
        {
            return AddIndent(this.value.ToString().ToLower(), indent, useOneSpaceIndent);
        }

        public static implicit operator bool(Json5Boolean value)
        {
            return value.value;
        }
    }
}
