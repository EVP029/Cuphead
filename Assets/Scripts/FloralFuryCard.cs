using UnityEngine;
using UnityEngine.SceneManagement;

public class FloralFuryCard : MonoBehaviour
{
    [SerializeField] private GameObject card;
    //[SerializeField] private string nombreEscena = "SampleScene";
    [SerializeField] private int numeroEscena;
    [SerializeField] private GameObject buttonA;
    [SerializeField] private Animator buttonAnimator;

    private bool jugadorCerca = false;
    private int paraEntrar = 0; //Para saber en qué clic está el jugador (y ejecute dependiendo del clic la acción)

    void Start()
    {
        card.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (jugadorCerca == false) //Esto es para que no entre al juego desde cualquier lugar del mapa, sólo al colisionar con el arbusto
        {
            return;
        }

        if (jugadorCerca && Input.GetButtonDown("Submit")) //Esto es para entrar al nivel, Submit funcionada para teclado (Enter) y control (Letra A)
        {
            ParaEntrarNivel();
        }

        if (Input.GetButtonDown("Cancel")) //Esto es por si ya no quiere entrar al nivel, Cancel funciona para teclado y control (Esc para teclado/B para control)
        {
            Regresar();
        }
    }

    void ParaEntrarNivel()
    {
        if (paraEntrar == 0)
        {
            card.SetActive(true); //Esto es para que se muestre la tarjeta al dar el primer clic
            paraEntrar = 1;
        }
        else if (paraEntrar == 1)
        {
            SceneManager.LoadScene(numeroEscena); //Esto es para entrar al nivel
        }
    }

    void Regresar()
    {
        if (paraEntrar == 1) //Se oculta la tarjeta si ya no quiere entrar
        {
            card.SetActive(false);
            paraEntrar = 0;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player")) //Esto es para que al colisionar con el arbusto, se active la opción de entrar al nivel (al presionar enter apareza la tarjeta del nivel)
        {
            jugadorCerca = true;
        }

        if (paraEntrar == 0)
        {
            buttonA.GetComponent<Animator>().SetBool("mostrar", true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) //Esto es por si el jugador colisiona con el arbusto para entrar con Cagney, pero no desea entrar, hace que el juego no esté esparando a que presione la tecla o botón
        {
            jugadorCerca = false;

            card.SetActive(false);

            if (paraEntrar == 0)
            {
                //Esto es que al momento de alejarse del trigger, no lo muestre
                buttonA.GetComponent<Animator>().SetBool("mostrar", false);
            }
            else
            {
                buttonA.GetComponent<Animator>().SetBool("mostrar", false);
            }
            paraEntrar = 0;
        }
    }
}
