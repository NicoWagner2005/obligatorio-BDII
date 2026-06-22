namespace TicketingMundialUCU.Data.Daos;

public record KpiAuditoria(
    int VentasPagas,
    int VentasPendientes,
    int VentasConfirmadas,
    decimal IngresosTotales,
    int TotalEntradas,
    int EntradasConsumidas,
    int TotalTransferencias,
    int TransferenciasAceptadas);

public record RankingEvento(
    int IdEvento,
    string EquipoLocal,
    string EquipoVisitante,
    string NombreEstadio,
    DateTime FechaHora,
    int EntradasVendidas,
    int EntradasConsumidas,
    decimal Ingresos,
    int CapacidadTotal)
{
    public decimal PctOcupacion => CapacidadTotal > 0
        ? Math.Round((decimal)EntradasVendidas / CapacidadTotal * 100, 1)
        : 0m;
}

public record RankingComprador(
    string Email,
    int TotalVentas,
    decimal TotalGastado,
    int TotalEntradas);

public record RankingFuncionario(
    string NroLegajo,
    string Email,
    int TotalValidaciones);

public record EstadisticaTransferencias(
    int Pendientes,
    int Aceptadas,
    int Rechazadas);

public interface IAuditoriaDao
{
    Task<KpiAuditoria> GetKpiAsync();
    Task<IEnumerable<RankingEvento>> GetRankingEventosAsync();
    Task<IEnumerable<RankingComprador>> GetRankingCompradoresAsync();
    Task<IEnumerable<RankingFuncionario>> GetRankingFuncionariosAsync();
    Task<EstadisticaTransferencias> GetEstadisticaTransferenciasAsync();
}
