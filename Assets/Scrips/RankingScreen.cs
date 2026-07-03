using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class RankingScreen : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private GameObject pantallaInscripcion;

    private VisualElement tablaRanking;
    private Label mensajeError;
    private int idTorneoActual;

    private void Awake()
    {
        var root = uiDocument.rootVisualElement;
        tablaRanking = root.Q<VisualElement>("tabla-ranking");
        mensajeError = root.Q<Label>("mensaje-error-ranking");
        mensajeError.style.display = DisplayStyle.None;

        var btnInscribirme = root.Q<Button>("btn-inscribirme");
        if (btnInscribirme != null)
            btnInscribirme.clicked += () => IrAInscripcion(idTorneoActual);
    }

    public void Mostrar(int idTorneo)
    {
        idTorneoActual = idTorneo;
        tablaRanking.Clear();
        mensajeError.style.display = DisplayStyle.None;
        StartCoroutine(CargarRanking(idTorneo));
    }

    private System.Collections.IEnumerator CargarRanking(int idTorneo)
    {
        string url = $"{ApiConfig.BaseUrl}ranking.php?id_torneo={idTorneo}";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                MostrarError("No se pudo cargar el ranking. Verifica tu conexión con el servidor.");
                yield break;
            }

            List<RankingEntry> ranking;
            try
            {
                ranking = JsonConvert.DeserializeObject<List<RankingEntry>>(request.downloadHandler.text);
            }
            catch (System.Exception e)
            {
                MostrarError("Error al leer el ranking.");
                Debug.LogError(e);
                yield break;
            }

            ConstruirTabla(ranking);
        }
    }

    private void ConstruirTabla(List<RankingEntry> ranking)
    {
        tablaRanking.Add(CrearFila("Puesto", "Jugador", "País", "Puntaje", esEncabezado: true));

        foreach (var fila in ranking)
        {
            string puesto = fila.puesto_final.HasValue ? fila.puesto_final.Value.ToString() : "-";
            tablaRanking.Add(CrearFila(puesto, fila.nombre_jugador, fila.pais, fila.puntaje_obtenido.ToString()));
        }
    }

    private VisualElement CrearFila(string puesto, string jugador, string pais, string puntaje, bool esEncabezado = false)
    {
        var fila = new VisualElement();
        fila.style.flexDirection = FlexDirection.Row;
        fila.style.paddingTop = 4;
        fila.style.paddingBottom = 4;

        fila.Add(CrearCelda(puesto, esEncabezado));
        fila.Add(CrearCelda(jugador, esEncabezado));
        fila.Add(CrearCelda(pais, esEncabezado));
        fila.Add(CrearCelda(puntaje, esEncabezado));

        return fila;
    }

    private Label CrearCelda(string texto, bool esEncabezado)
    {
        var label = new Label(texto);
        label.style.flexGrow = 1;
        label.style.color = esEncabezado ? new Color(0.4f, 0.75f, 1f) : Color.white;
        label.style.unityFontStyleAndWeight = esEncabezado ? FontStyle.Bold : FontStyle.Normal;
        label.style.fontSize = 14;
        return label;
    }

    private void MostrarError(string mensaje)
    {
        mensajeError.text = mensaje;
        mensajeError.style.display = DisplayStyle.Flex;
    }

    private void IrAInscripcion(int idTorneo)
    {
        gameObject.SetActive(false);
        pantallaInscripcion.SetActive(true);
        pantallaInscripcion.GetComponent<InscripcionForm>().idTorneoActual = idTorneo;
    }
}