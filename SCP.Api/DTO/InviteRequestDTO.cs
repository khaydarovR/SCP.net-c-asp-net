namespace SCP.Api.DTO
{
    public class InviteRequestDTO
    {
        /// <summary>
        /// Список id сейфов для которых применяется запрос
        /// </summary>
        public List<string> SafeIds { get; set; }
        /// <summary>
        /// Список id пользователей которым выдются разрешения
        /// </summary>
        public List<string> UserIds { get; set; }
        /// <summary>
        /// Список email если пользователь не имеет с вами общих сейфов
        /// </summary>
        public List<string> UserEmails { get; set; }
        /// <summary>
        /// Список slug ов для выдаваемых разрешений
        /// </summary>
        public List<string> Permisions { get; set; }
        /// <summary>
        /// Срок действия разрешения относитльно текущей даты
        /// </summary>
        public int DayLife { get; set; }
    }
}
