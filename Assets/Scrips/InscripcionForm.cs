using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Text;

public class InscripcionForm : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    private TextField inputNombre;
    private TextField inputPais;
    private TextField inputRango;
    private Button btnInscribir;
    private Label mensaje;

    public int idTorneoActual;

    private void OnEnable()
    {
        var root = uiDocument.rootVisualElement;
        inputNombre = root.Q<TextField>("input-nombre");
        inputPais = root.Q<TextField>("input-pais");
        inputRango = root.Q<TextField>("input-rango");
        btnInscribir = root.Q<Button>("btn-inscribir");
        mensaje = root.Q<Label>("mensaje-inscripcion");

        mensaje.style.display = DisplayStyle.None;
        btnInscribir.clicked += OnInscribirClicked;
    }

    private void OnDisable()
    {
        btnInscribir.clicked -= OnInscribirClicked;
    }

    private void OnInscribirClicked()
    {
        if (string.IsNullOrWhiteSpace(inputNombre.value) ||
            string.IsNullOrWhiteSpace(inputPais.value) ||
            string.IsNullOrWhiteSpace(inputRango.value))
        {
            MostrarMensaje("Completa todos los campos.", esError: true);
            return;
        }

        StartCoroutine(EnviarInscripcion());
    }

    private System.Collections.IEnumerator EnviarInscripcion()
    {
        var payload = new
        {
            id_torneo = idTorneoActual,
            nombre_jugador = inputNombre.value,
            pais = inputPais.value,
            rango = inputRango.value
        };

        string json = JsonConvert.SerializeObject(payload);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        string url = ApiConfig.BaseUrl + "inscripcion.php";
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                string errorTexto = "No se pudo registrar la inscripción.";
                try
                {
                    var err = JsonConvert.DeserializeObject<ApiError>(request.downloadHandler.text);
                    if (err != null && !string.IsNullOrEmpty(err.error))
                        errorTexto = err.error;
                }
                catch { /* si no viene JSON válido, usamos el mensaje genérico */ }

                MostrarMensaje(errorTexto, esError: true);
                yield break;
            }

            MostrarMensaje("¡Inscripción registrada correctamente!", esError: false);
            inputNombre.value = "";
            inputPais.value = "";
            inputRango.value = "";
        }
    }

    private void MostrarMensaje(string texto, bool esError)
    {
        mensaje.text = texto;
        mensaje.style.color = esError ? Color.red : Color.green;
        mensaje.style.display = DisplayStyle.Flex;
    }
}