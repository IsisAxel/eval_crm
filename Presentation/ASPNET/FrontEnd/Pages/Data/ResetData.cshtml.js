const App = {
    setup() {
        const state = Vue.reactive({
            isSubmitting: false,
        });

        const resetData = async () => {
            try {
                const response = await AxiosManager.get('/Data/ResetData', {});
                return response;
            } catch (error) {
                throw error;
            }
        }

        const handleSubmit = async () => {

            try {
                state.isSubmitting = true;
                await new Promise(resolve => setTimeout(resolve, 300));

                const response = await resetData();
                if (response.data.code === 200) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Data Reset Successfully',
                        text: 'You are being redirected...',
                        timer: 2000,
                        showConfirmButton: false
                    });
                    setTimeout(() => {
                        window.location.href = '/Dashboards/DefaultDashboard';
                    }, 2000);
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Data Reset Failed',
                        text: response.data.message ?? 'Failed to reset data.',
                        confirmButtonText: 'Try Again'
                    });
                }

            } catch (error) {
                Swal.fire({
                    icon: 'error',
                    title: 'An Error Occurred',
                    text: error.response?.data?.message || 'Please try again.',
                    confirmButtonText: 'OK'
                });
            } finally {
                state.isSubmitting = false;
            }
        };

        return {
            state,
            handleSubmit
        };
    }
};

Vue.createApp(App).mount('#app');

