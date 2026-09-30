using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR.OpenXR;

public class ControlDeEnemigo : MonoBehaviour
{
    private enum EstadoDeEnemigo
    {
        Patullar,
        Seguir
    }

    private static readonly int EstaCaminando = Animator.StringToHash(name: "EstaCaminando");

    [Header("Referencias")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] puntosDePatrulla;

    [Header("Configuracion")]
    [SerializeField] private float tiempoDeEsperaDePatrulla = 2.0f;
    [SerializeField] private float distanciaDeParada = 0.5f;
    [SerializeField] private float rangoDeteccion = 5f;
    [SerializeField] private float anguloVicion = 90f;
    [SerializeField] private float tiempoPerderJugador = 3f;

    private NavMeshAgent _agent;
    private Animator _animator;
    private EstadoDeEnemigo _state = EstadoDeEnemigo.Patullar;
    private int _currentPatrolIndex;
    private bool _isWaiting;
    private float _tiempoParaPerderJugador;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        IrAlSiguientePuntoDePatrulla();
    }

    private void Update()
    {
        var distanciaAlJugador = Vector3.Distance(player.position, transform.position);
        
        switch(_state)
        {

            case EstadoDeEnemigo.Patullar:
                Patrulla();
                if (distanciaAlJugador <= rangoDeteccion && EstaEnCampoDeVision())
                {
                    _state = EstadoDeEnemigo.Seguir;
                    _tiempoParaPerderJugador = tiempoPerderJugador;
                }
                break;
            case EstadoDeEnemigo.Seguir:
                SeguirJugador();
                if (!EstaEnCampoDeVision())
                {
                    _tiempoParaPerderJugador += Time.deltaTime;
                    if (_tiempoParaPerderJugador >= tiempoPerderJugador)
                    {
                        _state = EstadoDeEnemigo.Patullar;
                        IrAlCerrarPuntoDePatrulla();
                    }
                }
                else
                {
                    _tiempoParaPerderJugador = 0f;
                }
                break;
        }

        
        ActualizarAnimacion();
    }

    private void SeguirJugador() 
    {
     _agent.SetDestination(player.position);
    }

    private void Patrulla()
    {
        if (_isWaiting) return;
        if (!_agent.pathPending && _agent.remainingDistance <= distanciaDeParada)
        {
            StartCoroutine(EsperarPuntoPatrulla());
        }
    }

    private IEnumerator EsperarPuntoPatrulla()
    {
        _isWaiting = true;
        _agent.isStopped = true;
        yield return new WaitForSeconds(tiempoDeEsperaDePatrulla);

        _agent.isStopped = false;
        IrAlSiguientePuntoDePatrulla();
        _isWaiting = false;
    }

    private void IrAlCerrarPuntoDePatrulla()
    {
        if (puntosDePatrulla.Length == 0) return;
        var distanciaMinima = float.MaxValue;
        var indiceCercano = 0;
        for (var i = 0; i < puntosDePatrulla.Length; i++)
        {
            var distancia = Vector3.Distance(transform.position, puntosDePatrulla[i].position);
            if (distancia < distanciaMinima)
            {
                distanciaMinima = distancia;
                indiceCercano = i;
            }
        }
        _currentPatrolIndex = indiceCercano;
        _agent.SetDestination(puntosDePatrulla[_currentPatrolIndex].position);
    }

    private void IrAlSiguientePuntoDePatrulla()
    {
        if (puntosDePatrulla.Length == 0) return;

        _agent.SetDestination(puntosDePatrulla[_currentPatrolIndex].position);
        _currentPatrolIndex = (_currentPatrolIndex + 1) % puntosDePatrulla.Length;
    }

    private void ActualizarAnimacion()
    {
        var estaCaminando = _agent.velocity.sqrMagnitude > 0.01f;
        _animator.SetBool(EstaCaminando, estaCaminando);
    }

    private bool EstaEnCampoDeVision()
    {
       return EstaDeCaraALJugador() && EsClaroElCaminoAlJugador();

    }
    private bool EstaDeCaraALJugador()
    {
        Vector3 dirAJugador = (player.position - transform.position).normalized;
        var angulo = Vector3.Angle(transform.forward, dirAJugador);
        return angulo < anguloVicion / 2f;
    }
    
    private bool EsClaroElCaminoAlJugador()
    {
        var dirAJugador = player.position - transform.position;
        if (Physics.Raycast(transform.position, dirAJugador.normalized, out RaycastHit hit, dirAJugador.magnitude))
        {
            return hit.transform == player;
        }
        return true;
    }
}
