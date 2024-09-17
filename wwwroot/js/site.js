




   


let hamMenu = document.querySelector(".ContainerMenuHamburguer");

hamMenu.addEventListener("click", () => {
    hamMenu.classList.toggle("activeHamburguer");
})



//UlDisplay
    let UlBar = document.querySelector(".UlSideBar");
    let toggleButton = document.querySelector(".ContainerMenuHamburguer");

    toggleButton.addEventListener("click", () => {
        UlBar.classList.toggle("activeUlSideBar");
    });





const sidebar = document.querySelector('.sidebar');
const toggleBTN = document.querySelector('.toggle-btn');

toggleBTN.addEventListener('click', () => {
    sidebar.classList.toggle('activeBar');
   
})



//Metodo para Deslogearnos
document.getElementById('logoutLink').addEventListener('click', function (event) {
    event.preventDefault();
    document.getElementById('logoutForm').submit();
});



//Funcion Para Mostrar Contrasena
function mostrarPassword() {
    var x = document.getElementById("passwordInput");
    if (x.type === "password") {
        x.type = "text";
    } else {
        x.type = "password";
    }
}



function mostrarSweetAlert() {
    Swal.fire({
        icon: 'error',
        title: 'Error',
        text: 'Correo o Password incorrectos!',
        color: "red",
        confirmButtonText: "Ok, Thanks!",
        backdrop: `
    rgba(255,0,0,0.2)
   
  `

    });

    buttonsStyling: false



}


document.addEventListener('DOMContentLoaded', function () {
    // Verifica si el elemento con la clase 'alert-danger' está presente
    var alertaLogin = document.querySelector('.AlertaLogin');

    // Si está presente, muestra la alerta de SweetAlert
    if (alertaLogin) {
        mostrarSweetAlert();
    }
});