import { useNavigate } from 'react-router-dom';
import { useEffect } from 'react';
function Unauthenticated() {
    const navigate = useNavigate();

    const delayTransition = () => {
        setTimeout(() => {
            navigate("/");
        }, 3000);
    };

    useEffect(() => {
        delayTransition();
    }, []);

  return (
      <div className="page">
          <h1>
              You don't have permission to access this page. Returning to login page...
          </h1>
      </div>
  );
}

export default Unauthenticated;