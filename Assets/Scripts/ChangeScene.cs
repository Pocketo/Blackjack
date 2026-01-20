using UnityEngine;
using UnityEngine.SceneManagement;

namespace Blackjack
{
    public class ChangeScene : MonoBehaviour
    {
        public void Normal()
        {
            SceneManager.LoadScene("Normal");
        }
        public void Campana()
        {
            SceneManager.LoadScene("Campana");
        }
        public void Salir()
        {
            Application.Quit();
        }

        public void Menu()
        {
            SceneManager.LoadScene("Menu");
        }
    }
}
