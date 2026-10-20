import { useParams } from 'react-router-dom';

//Welcome page that displays after successful registeration

function Welcome() {
    const { username } = useParams();

  return (
      <div className="page">
          <h1>Welcome {username}</h1>
          <h2>Please check your email to confirm your account to gain access</h2>
      </div>
  );
}

export default Welcome;