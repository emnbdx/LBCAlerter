//------------------------------------------------------------------------------
// <auto-generated>
//     Ce code a été généré à partir d'un modèle.
//
//     Des modifications manuelles apportées à ce fichier peuvent conduire à un comportement inattendu de votre application.
//     Les modifications manuelles apportées à ce fichier sont remplacées si le code est régénéré.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EMToolBox.Mail
{
    using System;
    using System.Collections.Generic;
    
    public partial class QUEUE
    {
        public int ID { get; set; }
        public int PATTERN_ID { get; set; }
        public string TO { get; set; }
        public string SUBJECT { get; set; }
        public string BODY { get; set; }
        public Nullable<System.DateTime> CREATIONDATE { get; set; }
        public Nullable<System.DateTime> SENDDATE { get; set; }
        public bool SEND { get; set; }
    
        public virtual PATTERN PATTERN { get; set; }
    }
}
