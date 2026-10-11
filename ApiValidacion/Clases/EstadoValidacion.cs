namespace ValidacionEventos.Logica;

/// <summary>Un estado por cada motivo real de aceptación o rechazo en la puerta.</summary>
public enum EstadoValidacion
{
    Valida,
    NoExiste,
    YaFueUsada,
    EventoIncorrecto,
    EventoCancelado,
    Cancelada
}
