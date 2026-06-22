namespace TicketingMundialUCU.Data.Daos;

public record EventoRanking(
    int IdEvento,
    DateTime FechaHoraEvento,
    string NombreEstadio,
    string EquipoLocal,
    string EquipoVisitante,
    int EntradasVendidas,
    decimal Recaudacion,
    int CapacidadTotal);

public record CompradorRanking(
    string EmailUsuario,
    int TotalVentas,
    decimal TotalGastado,
    int TotalEntradas);

public record OcupacionSector(
    int IdEvento,
    DateTime FechaHoraEvento,
    string EquipoLocal,
    string EquipoVisitante,
    string NombreEstadio,
    string IdSector,
    int CapacidadMax,
    int EntradasVendidas,
    decimal PorcentajeOcupacion);

public record MovimientoAuditoria(
    int IdMovimiento,
    Guid IdEntrada,
    string TipoMovimiento,
    DateTime FechaMovimiento,
    string EmailPoseedorActual,
    string EquipoLocal,
    string EquipoVisitante,
    DateTime FechaHoraEvento,
    int? IdTransferencia);

public record ResumenEstadisticas(
    int TotalVentas,
    int VentasPendientes,
    int VentasConfirmadas,
    int VentasPagas,
    int TotalEntradas,
    decimal RecaudacionTotal,
    decimal RecaudacionPaga,
    int TotalUsuarios,
    int TotalEventos,
    int TotalTransferencias);

public interface IAuditoriaDao
{
    Task<ResumenEstadisticas> GetResumenAsync();
    Task<IEnumerable<EventoRanking>> GetRankingEventosAsync();
    Task<IEnumerable<CompradorRanking>> GetRankingCompradoresAsync();
    Task<IEnumerable<OcupacionSector>> GetOcupacionSectoresAsync();
    Task<IEnumerable<MovimientoAuditoria>> GetHistorialCustodiaAsync(int limite = 100);
}
