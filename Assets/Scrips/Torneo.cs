using System;

[Serializable]
public class Torneo
{
    public int id_torneo;
    public string nombre_torneo;
    public string juego;
    public string fecha_torneo;
    public decimal premio_usd;
}

[Serializable]
public class RankingEntry
{
    public int? puesto_final;
    public string nombre_jugador;
    public string pais;
    public int puntaje_obtenido;
}

[Serializable]
public class ApiError
{
    public string error;
}