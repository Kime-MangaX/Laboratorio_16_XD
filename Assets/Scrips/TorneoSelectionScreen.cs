using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class TorneoSelectionScreen : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private GameObject pantallaRanking;

    private ListView listaTorneos;
    private Label mensajeError;
    private List<Torneo> torneos = new List<Torneo>();

    private void OnEnable()
    {
        var root = uiDocument.rootVisualElement;
        listaTorneos = root.Q<ListView>("lista-torneos");
        mensajeError = root.Q<Label>("mensaje-error");

        mensajeError.style.display = DisplayStyle.None;

        ConfigurarListView();
        StartCoroutine(CargarTorneos());
    }

    private void ConfigurarListView()
    {
        listaTorneos.makeItem = () => new Label();
        listaTorneos.bindItem = (element, i) =>
        {
            var t = torneos[i];
            var label = element as Label;
            label.text = $"{t.nombre_torneo}  |  {t.juego}  |  {t.fecha_torneo}  |  ${t.premio_usd}";

            label.style.borderLeftWidth = 4;
            label.style.borderLeftColor = ColorPorJuego(t.juego);
            label.style.paddingLeft = 12;
        };
        listaTorneos.itemsSource = torneos;
        listaTorneos.selectionType = SelectionType.Single;
        listaTorneos.selectionChanged += (items) =>
        {
            foreach (var item in items)
            {
                var seleccionado = item as Torneo;
                if (seleccionado != null)
                    IrARanking(seleccionado.id_torneo);
                break;
            }
        };
    }

    private Color ColorPorJuego(string juego)
    {
        switch (juego)
        {
            case "Valorant":
                return new Color(1f, 0.27f, 0.33f);       // rojo Valorant
            case "CS2":
                return new Color(0.97f, 0.64f, 0f);        // dorado CS2
            case "League of Legends":
                return new Color(0.04f, 0.78f, 0.73f);     // teal LoL
            case "Dota 2":
                return new Color(0.76f, 0.15f, 0.22f);     // rojo vino Dota 2
            default:
                return new Color(0.4f, 0.75f, 1f);         // celeste por defecto
        }
    }

    private System.Collections.IEnumerator CargarTorneos()
    {
        string url = ApiConfig.BaseUrl + "torneos.php";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                MostrarError("No se pudo conectar con el servidor. Revisa que XAMPP esté corriendo.");
                yield break;
            }

            try
            {
                torneos = JsonConvert.DeserializeObject<List<Torneo>>(request.downloadHandler.text);
                listaTorneos.itemsSource = torneos;
                listaTorneos.RefreshItems();
            }
            catch (System.Exception e)
            {
                MostrarError("Error al leer los datos de torneos.");
                Debug.LogError(e);
            }
        }
    }

    private void MostrarError(string mensaje)
    {
        mensajeError.text = mensaje;
        mensajeError.style.display = DisplayStyle.Flex;
    }

    private void IrARanking(int idTorneo)
    {
        gameObject.SetActive(false);
        pantallaRanking.SetActive(true);
        pantallaRanking.GetComponent<RankingScreen>().Mostrar(idTorneo);
    }
}