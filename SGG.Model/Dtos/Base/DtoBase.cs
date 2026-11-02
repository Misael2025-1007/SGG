namespace SGG.Model.Dtos.Base
{
    /// <summary>
    /// Base de todos los DTO de entrada. UserId identifica al usuario que realiza la operación (se guarda en UserMod).
    /// </summary>
    public abstract class DtoBase
    {
        public int? UserId { get; set; }
    }
}
